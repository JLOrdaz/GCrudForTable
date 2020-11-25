CREATE PROCEDURE [dbo].[P_<tabla>_save]
<camposParametro>
AS
	IF (@<PK> IS NULL)
	BEGIN
		INSERT INTO <tabla>(<camposInsert>)
		VALUES(<camposParametroLinea>)

		SELECT SCOPE_IDENTITY() as Id, '<tablaUpper> saved' as Mensaje
	END
	ELSE
	BEGIN
		UPDATE <tabla>
		SET <camposUpdate>
		WHERE <PK> = @<PK>

		SELECT @<PK> as Id, '<tablaUpper> updated' as Mensaje
	END
GO
