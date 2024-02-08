using System.Text;

string rutaLocal = Path.GetDirectoryName(AppContext.BaseDirectory);
string rutaOutput = string.Empty;
string archivoJson = string.Empty;
StreamWriter writer;
Tabla tabla;

if (args.Count() == 2)
{
    archivoJson = args[0];
    rutaOutput = args[1];
}

CrearDataTabla();

writer = new StreamWriter(Path.Combine(rutaOutput, $"{tabla.Nombre}_CreateCRUD.sql"));
GenerateTable();
GenerateQuery();
GenerateDelete();
GenerateSave();
writer.Flush();
writer.Close();


writer = new StreamWriter(Path.Combine(rutaOutput, $"{tabla.Nombre}_Model.cs"));
GenerateClase();
writer.Flush();
writer.Close();

Console.WriteLine("Listo.");


void GenerateTable()
{
    Console.WriteLine("Creating table...");
    StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "CreateTable.sql"));
    string sql = reader.ReadToEnd();
    StringBuilder camposTabla = new StringBuilder();
    //<campostabla>
    foreach (var item in tabla.Campos)
    {
        camposTabla.Append("\n");
        camposTabla.Append($"    {item.Campo} {item.TipoDato} {(item.PK ? $"CONSTRAINT PK_{tabla.Nombre} PRIMARY KEY IDENTITY" : "NULL")},");
    }
    camposTabla = camposTabla.Remove(camposTabla.ToString().LastIndexOf(","), 1);

    //replace tags
    sql = sql.Replace("<tabla>", tabla.Nombre)
    .Replace("<camposTabla>", camposTabla.ToString());
    writer.WriteLine(sql);
}

void GenerateQuery()
{
    Console.WriteLine("Creating query procedure...");
    StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "PQuery.sql"));
    string sql = reader.ReadToEnd();
    StringBuilder camposTabla = new StringBuilder();
    string campoPK = tabla.Campos.Where(x => x.PK == true).FirstOrDefault().Campo;
    //<campos>
    foreach (var item in tabla.Campos)
    {
        camposTabla.Append($"{item.Campo},");
    }
    camposTabla = camposTabla.Remove(camposTabla.ToString().LastIndexOf(","), 1);

    sql = sql.Replace("<tabla>", tabla.Nombre)
            .Replace("<campos>", camposTabla.ToString())
            .Replace("<PK>", campoPK);

    writer.WriteLine(sql);
}

void GenerateSave()
{
    Console.WriteLine("Creating save procedure...");
    StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "PSave.sql"));
    string sql = reader.ReadToEnd();
    StringBuilder camposInsert = new StringBuilder();
    string campoPK = tabla.Campos.Where(x => x.PK == true).FirstOrDefault().Campo;
    //<camposInsert>
    foreach (var item in tabla.Campos)
    {
        camposInsert.Append($"{(!item.PK ? item.Campo + "," : "")}");
    }
    camposInsert = camposInsert.Remove(camposInsert.ToString().LastIndexOf(","), 1);

    StringBuilder camposUpdate = new StringBuilder();
    foreach (var item in tabla.Campos)
    {
        camposUpdate.Append($" {(!item.PK ? (item.Campo + "=@" + item.Campo + ",") : "")}");
    }
    camposUpdate = camposUpdate.Remove(camposUpdate.ToString().LastIndexOf(","), 1);

    StringBuilder camposValues = new StringBuilder();
    foreach (var item in tabla.Campos)
    {
        camposValues.Append($"{(!item.PK ? "@" + item.Campo + "," : "")}");
    }
    camposValues = camposValues.Remove(camposValues.ToString().LastIndexOf(","), 1);

    StringBuilder camposParametro = new StringBuilder();
    foreach (var item in tabla.Campos)
    {
        camposParametro.Append("\n");
        camposParametro.Append($"    @{item.Campo} {item.TipoDato}{(item.PK ? " = NULL," : ",")}");
    }
    camposParametro = camposParametro.Remove(camposParametro.ToString().LastIndexOf(","), 1);

    sql = sql.Replace("<tabla>", tabla.Nombre)
            .Replace("<camposInsert>", camposInsert.ToString())
            .Replace("<camposUpdate>", camposUpdate.ToString())
            .Replace("<camposValues>", camposValues.ToString())
            .Replace("<camposParametro>", camposParametro.ToString())
            .Replace("<tablaUpper>", UppercaseFirst(tabla.Nombre))
            .Replace("<PK>", campoPK);

    writer.WriteLine(sql);
}

void GenerateDelete()
{
    Console.WriteLine("Creating delete procedure...");
    StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "PDelete.sql"));
    string sql = reader.ReadToEnd();
    StringBuilder camposTabla = new StringBuilder();
    string campoPK = tabla.Campos?.Where(x => x.PK == true)?.FirstOrDefault()?.Campo ?? string.Empty;

    sql = sql.Replace("<tabla>", tabla.Nombre)
            .Replace("<tablaUpper>", UppercaseFirst(tabla.Nombre))
            .Replace("<PK>", campoPK);

    writer.WriteLine(sql);
}

void GenerateClase()
{
    Console.WriteLine("Generating Models...");
    StreamReader reader = new StreamReader(Path.Combine(rutaLocal!, "Templates", "Clase.txt"));
    string sql = reader.ReadToEnd();
    StringBuilder camposTabla = new StringBuilder();

    //<campos>
    foreach (var item in tabla.Campos!)
    {
        camposTabla.AppendLine($"\t public {ConvertSQLtoCS(item.TipoDato!)} {UppercaseFirst(item.Campo!)} {{get; set;}}");
    }

    sql = sql.Replace("<tabla>", UppercaseFirst(tabla.Nombre!))
            .Replace("<camposClase>", camposTabla.ToString());

    writer.WriteLine(sql);
}


string ConvertSQLtoCS(string tipo) => tipo.ToLower() switch
{
    "int" => "int",
    "bit" => "bool",
    string str when str.Contains("varchar") => "string",
    "money" => "decimal",
    _ => tipo.ToLowerInvariant()
};



void CrearDataTabla()
{
    StreamReader fileJson = new StreamReader(archivoJson);
    string json = fileJson.ReadToEnd();

    tabla = System.Text.Json.JsonSerializer.Deserialize<Tabla>(json);
}

string UppercaseFirst(string s)
{
    if (string.IsNullOrEmpty(s))
    {
        return string.Empty;
    }
    char[] a = s.ToCharArray();
    a[0] = char.ToUpper(a[0]);
    return new string(a);
}


