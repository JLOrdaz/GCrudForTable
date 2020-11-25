using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;

namespace GCrudForTable
{
    class Program
    {

        private static Tabla tabla;
        private static string rutaLocal = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        private static string rutaOutput;
        private static StreamWriter writer;
        public static string archivoJson;


        static void Main(string[] args)
        {
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
        }


        private static void GenerateTable()
        {
            StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "CreateTable.sql"));
            string sql = reader.ReadToEnd();
            StringBuilder camposTabla = new StringBuilder();
            //<campostabla>
            foreach (var item in tabla.Campos)
            {
                camposTabla.Append("\n");
                camposTabla.Append($"    {item.Campo} {item.TipoDato} {(item.PK ? "NOT NULL PRIMARY KEY" : "NULL")},");
            }
            camposTabla = camposTabla.Remove(camposTabla.ToString().LastIndexOf(","), 1);

            //replace tags
            sql = sql.Replace("<tabla>", tabla.Nombre)
            .Replace("<camposTabla>", camposTabla.ToString());
            writer.WriteLine(sql);
        }

        private static void GenerateQuery()
        {
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

        private static void GenerateSave()
        {
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

        private static void GenerateDelete()
        {
            StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "PDelete.sql"));
            string sql = reader.ReadToEnd();
            StringBuilder camposTabla = new StringBuilder();
            string campoPK = tabla.Campos.Where(x => x.PK == true).FirstOrDefault().Campo;

            sql = sql.Replace("<tabla>", tabla.Nombre)
                    .Replace("<tablaUpper>", UppercaseFirst(tabla.Nombre))
                    .Replace("<PK>", campoPK);

            writer.WriteLine(sql);
        }

        // private static void GenerateClase()
        // {
        //     StreamReader reader = new StreamReader(Path.Combine(rutaLocal, "Templates", "Clase.txt"));
        //     string sql = reader.ReadToEnd();
        //     StringBuilder camposTabla = new StringBuilder();

        //     sql = sql.Replace("<tabla>", tabla.Nombre)
        //             .Replace("<tablaUpper>", UppercaseFirst(tabla.Nombre))
        //             .Replace("<PK>", campoPK);

        //     writer.WriteLine(sql);
        // }


        private static void CrearDataTabla()
        {
            StreamReader fileJson = new StreamReader(archivoJson);
            string json = fileJson.ReadToEnd();

            tabla = System.Text.Json.JsonSerializer.Deserialize<Tabla>(json);
        }

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

    }
}
