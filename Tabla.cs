using System.Collections.Generic;

namespace GCrudForTable
{
    public class Tabla
    {
        public string Nombre { get; set; }
        public List<Campos> Campos {get;set;}

    }

    public class Campos
    {
        public string Campo { get; set; }
        public string TipoDato { get; set; }
        public bool PK { get; set; }
    }
}