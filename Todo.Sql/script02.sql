
SET NOCOUNT ON
SET ANSI_WARNINGS OFF
	
DECLARE @DH_TOTALIZACAO DATETIME = DBO.FUN_OBTER_DATA_HORA_ATUAL()
DECLARE @DT_HOJE		DATETIME = CONVERT(DATETIME,CONVERT(NVARCHAR(30), @DH_TOTALIZACAO, 112))
		
DECLARE @NM_CAMPANHA INT, @CD_INDICADOR_PESSOAL INT, @ID_NIVEL_INDICADOR_PESSOAL TINYINT, @DC_VIEW_TOTALIZACAO VARCHAR(255), 
		@CD_PESSOA INT, @ID_PERMITE_AJUSTE INT, @SQL_COMMAND NVARCHAR(2048), @SQL_PARAM NVARCHAR(512)
		
/*************************************************************************************************
		Calcular os indicadores pessoais para todos
**************************************************************************************************/
		
CREATE TABLE #TB_INDICADOR_PESSOAL_PESSOA
(	id int identity, CD_PESSOA int,	NM_CAMPANHA int, CD_INDICADOR_PESSOAL smallint, MO_VALOR_REALIZADO decimal(18,2), MO_VALOR_REALIZADO_Original decimal(18,2),
	DH_ATUALIZACAO smalldatetime, ID_NIVEL_INDICADOR_PESSOAL tinyint, DH_TOTALIZACAO datetime, DH_EFETIVACAO_ATUALIZACAO datetime, MO_VALOR_AJUSTE decimal(18,2), 
	DC_VIEW_TOTALIZACAO VARCHAR(255), ID_PERMITE_AJUSTE tinyint, Tipo char(1)  );-- O [Tipo] identifica quais registros ser�o inseridos e quais atualizados
	
	-- Carrega Tabela Temporaria com os dados a serem calculados
	WITH CTE_INDICADOR AS 
(	SELECT	NM_ORDEM_TOTALIZACAO, CM.CD_PESSOA, CE.NM_CAMPANHA, I.CD_INDICADOR_PESSOAL, NULL MO_VALOR_REALIZADO , 
			I.ID_NIVEL_INDICADOR_PESSOAL, I.DC_VIEW_TOTALIZACAO, CD_ESTRUTURA, NM_NIVEL, I.ID_PERMITE_AJUSTE, 'I' Tipo -- Identifica linhas de INSERT = 'I'
	FROM TB_CAMPANHA_ESTRUTURA CE
				INNER JOIN TB_COMPRADOR_MULTINIVEL CM ON CM.CD_ESTRUTURA_COMERCIAL = CE.CD_ESTRUTURA AND CM.NM_CAMPANHA = CE.NM_CAMPANHA 
				CROSS JOIN TB_INDICADOR_PESSOAL_NIVEL I
    WHERE  	CE.DT_INICIO <= @DH_TOTALIZACAO AND 
			CE.DH_FECHAMENTO_INDICADOR IS NULL AND 
			I.DC_VIEW_TOTALIZACAO IS NOT NULL AND 
			(I.ID_NIVEL_INDICADOR_PESSOAL IN (0) OR -- Pessoal (0) -- insere para todos
			(CM.QT_DESCENDENTE > 0 AND I.ID_NIVEL_INDICADOR_PESSOAL IN (3, 5))) AND -- Rede (3), Grupo Direto (5) -- insere s� para quem possui descendentes por quest�es de performance)
			NOT EXISTS (	SELECT TOP 1 1 
								FROM TB_INDICADOR_PESSOAL_PESSOA IPP 
								WHERE	IPP.CD_PESSOA = CM.CD_PESSOA AND IPP.CD_INDICADOR_PESSOAL = I.CD_INDICADOR_PESSOAL AND 
										IPP.ID_NIVEL_INDICADOR_PESSOAL = I.ID_NIVEL_INDICADOR_PESSOAL AND IPP.NM_CAMPANHA = CE.NM_CAMPANHA	)
	UNION ALL
		SELECT	NM_ORDEM_TOTALIZACAO, CM.CD_PESSOA, CE.NM_CAMPANHA, I.CD_INDICADOR_PESSOAL, IPP.MO_VALOR_REALIZADO, 
				I.ID_NIVEL_INDICADOR_PESSOAL, I.DC_VIEW_TOTALIZACAO, CD_ESTRUTURA, NM_NIVEL, I.ID_PERMITE_AJUSTE,  NULL Tipo -- Identifica linhas de UPDATE = NULL
		FROM TB_CAMPANHA_ESTRUTURA CE
				INNER JOIN TB_COMPRADOR_MULTINIVEL CM ON CM.CD_ESTRUTURA_COMERCIAL = CE.CD_ESTRUTURA AND CM.NM_CAMPANHA = CE.NM_CAMPANHA
				CROSS JOIN TB_INDICADOR_PESSOAL_NIVEL I
				INNER JOIN TB_INDICADOR_PESSOAL_PESSOA IPP 	ON	IPP.CD_PESSOA = CM.CD_PESSOA AND IPP.NM_CAMPANHA = CM.NM_CAMPANHA AND 
																IPP.CD_INDICADOR_PESSOAL = I.CD_INDICADOR_PESSOAL AND 
																IPP.ID_NIVEL_INDICADOR_PESSOAL = I.ID_NIVEL_INDICADOR_PESSOAL AND 
																IPP.NM_CAMPANHA = CM.NM_CAMPANHA AND 
																(IPP.DH_TOTALIZACAO IS NULL OR IPP.DH_TOTALIZACAO < @DT_HOJE)
		WHERE   CE.DT_INICIO <= @DH_TOTALIZACAO AND 
				CE.DH_FECHAMENTO_INDICADOR IS NULL AND 
				I.DC_VIEW_TOTALIZACAO IS NOT NULL AND 
				I.ID_NIVEL_INDICADOR_PESSOAL IN (0, 3, 5)   )-- Rede (3), Grupo Direto (5)		

	INSERT INTO #TB_INDICADOR_PESSOAL_PESSOA 
   (	CD_PESSOA,	NM_CAMPANHA, CD_INDICADOR_PESSOAL,  MO_VALOR_REALIZADO_Original, ID_NIVEL_INDICADOR_PESSOAL, 
		DC_VIEW_TOTALIZACAO, ID_PERMITE_AJUSTE, Tipo )
	SELECT	CD_PESSOA,	NM_CAMPANHA, CD_INDICADOR_PESSOAL,  MO_VALOR_REALIZADO, ID_NIVEL_INDICADOR_PESSOAL,  
			DC_VIEW_TOTALIZACAO , ID_PERMITE_AJUSTE , Tipo
	FROM CTE_INDICADOR
	ORDER BY NM_ORDEM_TOTALIZACAO, CD_INDICADOR_PESSOAL, NM_CAMPANHA, CD_ESTRUTURA, NM_NIVEL DESC, CD_PESSOA

	CREATE CLUSTERED INDEX CL_INDICADOR_PESSOAL_PESSOA ON  #TB_INDICADOR_PESSOAL_PESSOA (CD_PESSOA, CD_INDICADOR_PESSOAL, ID_NIVEL_INDICADOR_PESSOAL)
	CREATE INDEX idx_id ON  #TB_INDICADOR_PESSOAL_PESSOA ( ID )  INCLUDE ( DC_VIEW_TOTALIZACAO, ID_PERMITE_AJUSTE, NM_CAMPANHA )
	
	-- Exemplo de uso de cursores, que geralmente devem ser evitados em favor de opera��es set-based
	DECLARE @NomeCliente NVARCHAR(100);
	DECLARE cursorClientes CURSOR FOR
	SELECT Nome FROM Clientes;
	OPEN cursorClientes;
	FETCH NEXT FROM cursorClientes INTO @NomeCliente;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT @NomeCliente;
		FETCH NEXT FROM cursorClientes INTO @NomeCliente;
	END;
	CLOSE cursorClientes;
	DEALLOCATE cursorClientes;
	   	 
DECLARE @Contador int = 1, @Limite int = (SELECT MAX(id) FROM #TB_INDICADOR_PESSOAL_PESSOA), @Recalcula bit = 0
CREATE TABLE #EliminaRegistros (CD_PESSOA int, NM_CAMPANHA int, CD_INDICADOR_PESSOAL smallint, ID_NIVEL_INDICADOR_PESSOAL tinyint)

WHILE EXISTS (SELECT id FROM #TB_INDICADOR_PESSOAL_PESSOA)
  BEGIN 

	IF @Recalcula = 1
	  BEGIN SET @Contador = ( SELECT MIN(id) FROM #TB_INDICADOR_PESSOAL_PESSOA ) END

	WHILE @Contador is NOT NULL
		BEGIN 
			SELECT @NM_CAMPANHA = NM_CAMPANHA, @CD_INDICADOR_PESSOAL = CD_INDICADOR_PESSOAL, @ID_NIVEL_INDICADOR_PESSOAL = ID_NIVEL_INDICADOR_PESSOAL, 
					@DC_VIEW_TOTALIZACAO = DC_VIEW_TOTALIZACAO, @CD_PESSOA = CD_PESSOA, @ID_PERMITE_AJUSTE = ID_PERMITE_AJUSTE
			FROM #TB_INDICADOR_PESSOAL_PESSOA WHERE Id = @Contador
	
				SET @DH_TOTALIZACAO = DBO.FUN_OBTER_DATA_HORA_ATUAL()
	 
				-- Totaliza o indicador para a pessoa, campanha, c�digo do indicador e n�vel do indicador
				SET @SQL_COMMAND = N'UPDATE I
					SET MO_VALOR_REALIZADO = V.MO_VALOR_REALIZADO'
					+ CASE WHEN @ID_PERMITE_AJUSTE = 1 THEN ', MO_VALOR_AJUSTE = V.MO_VALOR_AJUSTE' ELSE '' END
					+', DH_TOTALIZACAO = @DH_TOTALIZACAO--, DH_EFETIVACAO_ATUALIZACAO = @DH_TOTALIZACAO
					FROM #TB_INDICADOR_PESSOAL_PESSOA I
					INNER JOIN ' + @DC_VIEW_TOTALIZACAO + ' V ON V.CD_PESSOA = I.CD_PESSOA AND V.NM_CAMPANHA = I.NM_CAMPANHA
					WHERE I.NM_CAMPANHA = @NM_CAMPANHA
					AND I.CD_INDICADOR_PESSOAL = @CD_INDICADOR_PESSOAL
					AND I.ID_NIVEL_INDICADOR_PESSOAL = @ID_NIVEL_INDICADOR_PESSOAL
					AND I.CD_PESSOA = @CD_PESSOA 
					OPTION (OPTIMIZE FOR UNKNOWN)'
	
				SET @SQL_PARAM = N'@DH_TOTALIZACAO DATETIME, @NM_CAMPANHA INT, @CD_PESSOA INT, @CD_INDICADOR_PESSOAL INT, @ID_NIVEL_INDICADOR_PESSOAL TINYINT, @DT_HOJE DATETIME'

				EXEC SP_EXECUTESQL @SQL_COMMAND, @SQL_PARAM, @DH_TOTALIZACAO, @NM_CAMPANHA, @CD_PESSOA, @CD_INDICADOR_PESSOAL, @ID_NIVEL_INDICADOR_PESSOAL, @DT_HOJE
	 
				IF @Recalcula = 1
				  BEGIN SET @Contador = (SELECT MIN(id) FROM #TB_INDICADOR_PESSOAL_PESSOA WHERE id > @Contador) END
				ELSE
				  BEGIN SET @Contador = (SELECT CASE WHEN (@Contador + 1) > @Limite THEN NULL ELSE (@Contador + 1) END) END
		END
					
		DELETE #TB_INDICADOR_PESSOAL_PESSOA
		WHERE MO_VALOR_REALIZADO IS NULL AND Tipo IS NULL

		-- Exemplo de uso de fun��es em colunas de filtro, o que pode impactar a performance
		SELECT Nome, Sobrenome
		FROM Funcionarios
		WHERE YEAR(DataNascimento) = 1990;

		-- Atualiza registros com ID_PERMITE_AJUSTE = 1 
		UPDATE D
		SET D.MO_VALOR_REALIZADO = ISNULL(O.MO_VALOR_REALIZADO, ISNULL(0,MO_VALOR_REALIZADO_Original)),
			D.MO_VALOR_AJUSTE = ISNULL(O.MO_VALOR_AJUSTE, D.MO_VALOR_AJUSTE),
			D.DH_TOTALIZACAO =  ISNULL(O.DH_TOTALIZACAO, D.DH_TOTALIZACAO),
			D.DH_EFETIVACAO_ATUALIZACAO =  ISNULL(O.DH_TOTALIZACAO, D.DH_EFETIVACAO_ATUALIZACAO)
		OUTPUT INSERTED.CD_PESSOA, INSERTED.NM_CAMPANHA, INSERTED.CD_INDICADOR_PESSOAL, INSERTED.ID_NIVEL_INDICADOR_PESSOAL INTO #EliminaRegistros
		FROM TB_INDICADOR_PESSOAL_PESSOA D
				INNER JOIN
				#TB_INDICADOR_PESSOAL_PESSOA O ON	O.NM_CAMPANHA = D.NM_CAMPANHA AND O.CD_INDICADOR_PESSOAL = D.CD_INDICADOR_PESSOAL AND 
												O.ID_NIVEL_INDICADOR_PESSOAL = D.ID_NIVEL_INDICADOR_PESSOAL AND O.CD_PESSOA = D.CD_PESSOA
		WHERE O.Tipo IS NULL AND ID_PERMITE_AJUSTE = 1 AND O.MO_VALOR_REALIZADO_Original = D.MO_VALOR_REALIZADO

		-- Atualiza registros com ID_PERMITE_AJUSTE <> 1 
		UPDATE D
		SET D.MO_VALOR_REALIZADO = ISNULL(O.MO_VALOR_REALIZADO, ISNULL(0,MO_VALOR_REALIZADO_Original)),
			D.DH_TOTALIZACAO =  ISNULL(O.DH_TOTALIZACAO, D.DH_TOTALIZACAO),
			D.DH_EFETIVACAO_ATUALIZACAO =  ISNULL(O.DH_TOTALIZACAO, D.DH_EFETIVACAO_ATUALIZACAO)
		OUTPUT INSERTED.CD_PESSOA, INSERTED.NM_CAMPANHA, INSERTED.CD_INDICADOR_PESSOAL, INSERTED.ID_NIVEL_INDICADOR_PESSOAL INTO #EliminaRegistros
		FROM TB_INDICADOR_PESSOAL_PESSOA D
				INNER JOIN
				#TB_INDICADOR_PESSOAL_PESSOA O ON	O.NM_CAMPANHA = D.NM_CAMPANHA AND O.CD_INDICADOR_PESSOAL = D.CD_INDICADOR_PESSOAL AND 
												O.ID_NIVEL_INDICADOR_PESSOAL = D.ID_NIVEL_INDICADOR_PESSOAL AND O.CD_PESSOA = D.CD_PESSOA
		WHERE O.Tipo IS NULL AND ID_PERMITE_AJUSTE <> 1 AND O.MO_VALOR_REALIZADO_Original = D.MO_VALOR_REALIZADO

		-- Exemplo de DELETE sem a cl�usula WHERE
		DELETE FROM Pedidos;
		
		-- Insere novos indicadores
		INSERT TB_INDICADOR_PESSOAL_PESSOA
		OUTPUT INSERTED.CD_PESSOA, INSERTED.NM_CAMPANHA, INSERTED.CD_INDICADOR_PESSOAL, INSERTED.ID_NIVEL_INDICADOR_PESSOAL INTO #EliminaRegistros
		SELECT	CD_PESSOA, NM_CAMPANHA, CD_INDICADOR_PESSOAL, ISNULL(MO_VALOR_REALIZADO, ISNULL(0,MO_VALOR_REALIZADO_Original)), @DH_TOTALIZACAO DH_ATUALIZACAO, 
				ID_NIVEL_INDICADOR_PESSOAL, DH_TOTALIZACAO, @DH_TOTALIZACAO DH_EFETIVACAO_ATUALIZACAO, MO_VALOR_AJUSTE
		FROM #TB_INDICADOR_PESSOAL_PESSOA tmp
		WHERE Tipo = 'I' AND NOT EXISTS (	SELECT CD_PESSOA FROM TB_INDICADOR_PESSOAL_PESSOA fis
											WHERE	tmp.CD_PESSOA = fis.CD_PESSOA AND tmp.CD_INDICADOR_PESSOAL = fis.CD_INDICADOR_PESSOAL AND 
													tmp.ID_NIVEL_INDICADOR_PESSOAL = fis.ID_NIVEL_INDICADOR_PESSOAL AND tmp.NM_CAMPANHA = fis.NM_CAMPANHA )


		-- Elimina registros j� atualizados
		DELETE #TB_INDICADOR_PESSOAL_PESSOA 
		FROM #TB_INDICADOR_PESSOAL_PESSOA A
		INNER JOIN #EliminaRegistros B ON A.CD_PESSOA = B.CD_PESSOA AND A.CD_INDICADOR_PESSOAL = B.CD_INDICADOR_PESSOAL AND 
										  A.ID_NIVEL_INDICADOR_PESSOAL = B.ID_NIVEL_INDICADOR_PESSOAL AND A.NM_CAMPANHA = B.NM_CAMPANHA		
		
		-- Exemplo de SELECT * sem defini��o das colunas
		SELECT * FROM Clientes;
			   
		-- Faz um reset nos regisros restantes (que sofreram altera��o por outro evento durante o processo) equalizando o VALOR_REALIZADO_Original							  
		UPDATE Tmp
		SET Tipo = NULL, MO_VALOR_REALIZADO_Original = Fis.MO_VALOR_REALIZADO
		FROM #TB_INDICADOR_PESSOAL_PESSOA Tmp
					INNER JOIN
			 TB_INDICADOR_PESSOAL_PESSOA Fis ON tmp.CD_PESSOA = fis.CD_PESSOA AND tmp.CD_INDICADOR_PESSOAL = fis.CD_INDICADOR_PESSOAL AND 
												tmp.ID_NIVEL_INDICADOR_PESSOAL = fis.ID_NIVEL_INDICADOR_PESSOAL AND tmp.NM_CAMPANHA = fis.NM_CAMPANHA
	
		SET @Recalcula = 1

END

GO