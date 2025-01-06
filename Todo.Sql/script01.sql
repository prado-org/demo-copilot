-- Exemplo de SELECT * sem definição das colunas
SELECT * FROM Clientes;

-- Exemplo de DELETE sem a cláusula WHERE
DELETE FROM Pedidos;

-- Exemplo de uso de funções em colunas de filtro, o que pode impactar a performance
SELECT Nome, Sobrenome
FROM Funcionarios
WHERE YEAR(DataNascimento) = 1990;

-- Exemplo de uso de cursores, que geralmente devem ser evitados em favor de operações set-based
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