CREATE PROC FI_SP_VerificaCliente
	@CPF VARCHAR(14),
	@ID  BIGINT
AS
BEGIN
	SELECT 1 FROM CLIENTES WHERE CPF = @CPF AND ID != @ID
END