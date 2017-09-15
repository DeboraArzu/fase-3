using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fase_3_Compiladores__tablas_
{
    class Tabla_Producciones
    {
        Dictionary<string, int> Valores;
        Dictionary<int, ElementoProduccion> ContenidoProduc;
        Dictionary<string, DatosTabla1> Simbolos;

        public Tabla_Producciones(Dictionary<string, int> Val, Dictionary<string, DatosTabla1> Simbo, Dictionary<int, ElementoProduccion> ContenidoProduccion)
        {
            Valores = Val;
            Simbolos = Simbo;
            ContenidoProduc = ContenidoProduccion;
        }

        public List<List<int>> Elementos()
        {
            List<List<int>> p = new List<List<int>>();
            List<int> con;
            foreach (var item in this.ContenidoProduc.Values)
            {
                List<List<string>> conte = item.Get_Contenindo();
                foreach (var val in conte)
                {
                    List<string> ob = val.ToList<string>();
                    con = new List<int>();
                    foreach (string i in ob)
                    {
                        if (EsTerminal(i))
                        {
                            var T = Valores [i];
                            con.Add(T * (-1));
                        }
                        else
                        {
                            if (i.Equals("ԑ"))
                            {
                                con.Add(0);
                            }
                            else
                            {
                                if (!EsAccion(i))
                                {
                                    var T = Simbolos [i].numtoken;
                                    con.Add(T);
                                }
                            }
                        }
                    }
                    p.Add(con);
                }
            }
            return p;
        }

        public Boolean EsTerminal(string asd)
        {
            char [] q = asd.ToArray();
            if (q [0] == '<' && q [q.Length - 1] == '>')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean EsAccion(string asd)
        {
            char [] q = asd.ToArray();
            if (q [0] == '{' && q [q.Length - 1] == '}')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    class Table3
    {
        public int longitud;
        public List<List<int>> numeros;

        public Table3(int longi, List<List<int>> nums)
        {
            longitud = longi;
            numeros = nums;
        }
    }
}
