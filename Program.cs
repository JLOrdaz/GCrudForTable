using System;
using System.IO;
using System.Text;
using System.Linq;

namespace GCrudForTable
{
    class Program
    {

        private static Tabla tabla;
        private static string rutaLocal = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        private static string rutaOutput;
        private static StreamWriter writer;


        static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                rutaOutput = args[0];
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
                camposUpdate.Append($"{(!item.PK ? (item.Campo + "=@" + item.Campo + ",") : "")}");
            }
            camposUpdate = camposUpdate.Remove(camposUpdate.ToString().LastIndexOf(","), 1);

            sql = sql.Replace("<tabla>", tabla.Nombre)
                    .Replace("<camposInsert>", camposInsert.ToString())
                    .Replace("<camposUpdate>", camposUpdate.ToString())
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

        private static void CrearDataTabla()
        {
            tabla = new Tabla();
            tabla.Nombre = "visit";
            tabla.Campos = new System.Collections.Generic.List<Campos>()
            {
                new Campos() { Campo = "visId", PK = true, TipoDato = "INT"},
                new Campos() { Campo = "usrId", PK = false, TipoDato = "INT"},
                new Campos() { Campo = "proId", PK = false, TipoDato = "INT"},
                new Campos() { Campo = "visDate", PK = false, TipoDato = "DATETIME"}
            };
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
