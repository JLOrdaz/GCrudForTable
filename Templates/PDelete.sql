CREATE PROCEDURE [dbo].[P_<tabla>_delete]
	@<PK> int
AS
	DELETE FROM <tabla> WHERE <PK> = @<PK>

	SELECT @<PK> as Id, '<tablaUpper> deleted' as Mensaje
GO

