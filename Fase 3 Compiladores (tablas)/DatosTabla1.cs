using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fase_3_Compiladores__tablas_
{
    class DatosTabla1
    {
        public int numtoken;
        public int precedencia;
        public string Asociatividad;
        public DatosTabla1(int num, int prece, string aso)
        {
            this.numtoken = num; ;
            this.precedencia = prece;
            this.Asociatividad = aso;
        }
    }
}
