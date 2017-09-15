using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fase_3_Compiladores__tablas_
{
    class TablaNo_Terminales
    {
        List<int> llavesSinStar;
        List<int> llavesConStar;
        Dictionary<int, ElementoProduccion> ContenidoProduccion;
        Dictionary<string, int> Valores = new Dictionary<string, int>();
        public TablaNo_Terminales(Dictionary<int, ElementoProduccion> t)
        {
            ContenidoProduccion = t;
            llavesSinStar = new List<int>();
            llavesConStar = new List<int>();
            this.ArreglarContenido();
        }
        public Dictionary<int, ElementoProduccion> getContenidoProduccion()
        {
            return ContenidoProduccion;
        }

        public Dictionary<string, int> getVaLores()
        {
            return Valores;
        }

        #region Tabla de No Termianles
        #region Coloca los Start con llaves iniciales
        void ArreglarContenido()
        {//metodo para arreglar el contenido
            int [] llaves = this.ContenidoProduccion.Keys.ToArray();//copia de las llaves
            foreach (int i in llaves)
            {
                ElementoProduccion iterator = this.ContenidoProduccion [i];
                string s = iterator.Get_llave();
                if (s.CompareTo("<start>") == 0)
                {
                    llavesConStar.Add(i);
                }
                else
                    llavesSinStar.Add(i);
            }
            Dictionary<int, ElementoProduccion> nuevo = new Dictionary<int, ElementoProduccion>();
            List<int> temp1 = new List<int>();
            List<int> temp2 = new List<int>();
            int llavesStar = 1;//comienza a asignar desde 1 a las producciones que comeincen con start
            int llaves_Sin_Star = llavesConStar.Count() + 1;//contador de llaves sin <Start>, inician despues de contar cuantas producciones poseen start
            foreach (int i in llaves)
            {
                if (llavesConStar.Contains(i))
                {
                    nuevo.Add(llavesStar, this.ContenidoProduccion [i]);
                    temp1.Add(llavesStar);
                    llavesStar++;
                }
                else
                {
                    nuevo.Add(llaves_Sin_Star, this.ContenidoProduccion [i]);
                    temp2.Add(llaves_Sin_Star);
                    llaves_Sin_Star++;
                }
            }
            this.ContenidoProduccion = nuevo;
            this.llavesConStar.Clear();
            this.llavesSinStar.Clear();
            this.llavesConStar = temp1;//se asignan las nuevas llaves a las listas de control 
            this.llavesSinStar = temp2;//para las que poseen las llaves de los que tienen start y los que no
        }
        #endregion
        public List<string> CalcularNoTerminales()
        {//metodo a corregir metodo a veces funciona a veces no
            int [] llaves = this.ContenidoProduccion.Keys.ToArray();
            Dictionary<int, string> datos = new Dictionary<int, string>();
            List<string> resp = new List<string>();
            List<string> pendientes = new List<string>();
            List<int> subllaves = new List<int>();
            foreach (int item in llaves)
            {
                ElementoProduccion iterator = this.ContenidoProduccion [item];
                string s = iterator.Get_llave();//la llave que devuelve es el <id> en minusculas
                if (!datos.ContainsValue(s))
                {
                    datos.Add(item, s);
                    subllaves.Add(item);
                }

            }
            foreach (int item in subllaves)
            {//se asignan a los que ya fueron instanciados en la gramatica en formato numero, no terminal, first, produccion
                string s = item.ToString() + '‡' + datos [item].ToString() + '‡' + " " + '‡' + " ";
                resp.Add(s);
            }
            int [] ar1 = subllaves.ToArray();
            int subllaveError = -1;
            foreach (int i in ar1)
            {
                if (i > subllaveError)
                {
                    subllaveError = i;
                }
            }
            if (subllaveError < 0)
            {
                subllaveError = 0;
            }
            subllaveError++;//sieguiente llave para los pendientes o los de error, como los quiera ver uno
            foreach (string item in resp)
            {//para obtener los <pendientes>, los que no se declararon con ->
                String [] r = item.Split('‡');
                int i = Convert.ToInt32(r [0]);
                ElementoProduccion iterator = this.ContenidoProduccion [i];
                List<string> contenido = iterator.GetContenido_comoUNAUNIcaLista();
                foreach (string x in contenido)
                {
                    bool b = (this.Es_MenorQ_ID_MayorQ(x));
                    if (b == true && datos.Values.Contains(x) == false && pendientes.Contains(x) == false)
                    {
                        pendientes.Add(x);
                    }
                }
            }
            String [] raux = pendientes.ToArray();
            int c = 0;
            foreach (string i in raux)
            {
                raux [c] = subllaveError.ToString() + '‡' + raux [c] + '‡' + " " + '‡' + "ERROR";
                subllaveError++;
                c++;
            }
            pendientes.Clear();
            pendientes = raux.ToList();
            c = 1;
            //calular la produccion de los demas
            string [] subresp = resp.ToArray();
            int icc = 0;
            foreach (string st in subresp)//dentro de un foreach para que lo haga con cada elemetno
            {
                string [] myr = st.Split('‡');//
                myr [3] = get_NoProduccion(myr [1], c, llaves);
                subresp [icc] = myr [0] + '‡' + myr [1] + '‡' + myr [2] + '‡' + myr [3];
                Valores.Add(myr [1], Convert.ToInt32(myr [0]));
                icc++;
                c++;
            }
            //luego aca concatenar y retornar
            resp.Clear();
            resp = subresp.ToList();
            resp = this.OrdenarResp(resp);
            string [] pendientealfinal = pendientes.ToArray();
            foreach (string i in pendientealfinal)
            {
                string [] myr = i.Split('‡');
                Valores.Add(myr [1], Convert.ToInt32(myr [0]));
                resp.Add(i);
            }

            return resp;
        }
        bool Es_MenorQ_ID_MayorQ(string s)//<id>
        {
            int A = 65;
            int a = 97;
            int Z = 90;
            int z = 122;
            int _0 = 48;
            int _9 = 57;
            int _ = 95;
            char [] palabra = s.ToCharArray();
            try
            {
                int contador = 0, otro = palabra.Length - 1;
                bool inicio, final, contenido;
                inicio = contenido = final = false;
                foreach (char c in palabra)
                {
                    if (contador == 0 && (c == '<'))
                    {
                        inicio = true;
                    }
                    else if ((contador == 0 && (c != '<')))
                    {
                        inicio = false;
                    }
                    else if (contador == 1 && contador != otro && ((c >= a && c <= z) || (c >= A && c <= Z)))
                    {
                        contenido = true;
                    }
                    else if ((contador == 1 && contador != otro && !((c >= a && c <= z) || (c >= A && c <= Z))))
                    {
                        contenido = false;
                    }
                    else if (contador > 1 && contador != otro && ((c >= a && c <= z) || (c >= A && c <= Z) || (c >= _0 && c <= _9) || c == _))
                    {
                        contenido = true;
                    }
                    else if ((contador > 1 && contador != otro && !((c >= a && c <= z) || (c >= A && c <= Z) || (c >= _0 && c <= _9) || c == _)))
                    {
                        contenido = false;
                    }
                    else if (contador > 1 && contador == otro && c == '>')
                    {
                        final = true;
                    }
                    else if ((contador > 1 && contador == otro && c != '>'))
                    {
                        final = false;
                    }
                    contador++;
                }

                if (inicio == true && contenido == inicio && final == inicio)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        List<string> OrdenarResp(List<string> t)
        {
            List<string> listaOrdenada = new List<string>();
            int [] subllabesStart = this.llavesConStar.ToArray();
            if (t.Count != 0)
            {
                List<int> control = new List<int>();
                foreach (int i in subllabesStart)//deprimero ordenamos en base a las llaves de los start
                {
                    foreach (string x in t)
                    {
                        string [] myr = x.Split('‡');//separa el contenido
                        int aux = Convert.ToInt32(myr [0]);
                        if (aux == i && control.Contains(i) == false)
                        {
                            control.Add(i);
                            listaOrdenada.Add(x);
                        }
                    }
                }
                subllabesStart = this.llavesSinStar.ToArray();//se recicla el mismo arreglo, solo que lo lleno de las otras laves
                foreach (int i in subllabesStart)//luego de los que no posean start
                {
                    foreach (string x in t)
                    {
                        string [] myr = x.Split('‡');//separa el contenido
                        int aux = Convert.ToInt32(myr [0]);
                        if (aux == i && control.Contains(i) == false)
                        {
                            control.Add(i);
                            listaOrdenada.Add(x);
                        }
                    }
                }
                t = listaOrdenada;
            }
            return t;
        }
        string get_NoProduccion(string s, int v, int [] llaves)
        { //retorna el # de la produccion de la cadena enviada, asumiendo que se envia <id>
            try
            {
                int produccion = v;
                foreach (int item in llaves)
                {//numero, no terminal, first, produccion
                    ElementoProduccion iterator = this.ContenidoProduccion [item];
                    string keu = iterator.Get_llave().ToLower();
                    if (keu.CompareTo(s) == 0)
                    {
                        return produccion.ToString();
                    }
                    string aux = iterator.EnqNivelLoPosee(s, produccion);

                    produccion = Convert.ToInt32(aux) - 1;
                }
                return produccion.ToString();
            }
            catch (Exception)
            {
                return "";
                throw;
            }

        }
        #endregion
        #region Calculador de produccion y siguiente produccion <int,int> = <produccion,siguiente>
        int Get_No_SiguienteProduccionRepetida(int intllave, string palabra)
        {
            int resp = 1;
            int [] llaves = this.ContenidoProduccion.Keys.ToArray();
            foreach (int i in llaves)
            {
                ElementoProduccion iterator = this.ContenidoProduccion [i];
                if (iterator.Get_llave().ToLower().CompareTo(palabra) == 0 && intllave == i)
                {
                    return resp;
                }
                resp = iterator.Calculador_de_nivelesV2(resp);

                resp++;
            }
            return resp;
        }
        public Dictionary<int, int> CalcularProducciones()
        {
            int [] llaves = this.ContenidoProduccion.Keys.ToArray();//contiene todo sobre las llaves, sin importar si es o no repetida
            Dictionary<int, string> datos = new Dictionary<int, string>();//no repetidos
            List<int> intsubllaves = new List<int>();//llaves enteras de los elementos no repetidos
            List<int> intLlavesDeLosRepetidos = new List<int>();//llaves int de los repetidos
            List<string> subllaves = new List<string>();//llaves de los elementos no repetidos
            List<string> llavesStrdelosRepetidos = new List<string>();//llaves de los repetidos
            Dictionary<int, int> RESPUESTA = new Dictionary<int, int>();
            foreach (int item in llaves)
            {//se leen las producciones y se obtienen por separado las llaves de los no repetidos y de los repetidos, pero loas llaves <id> del diccionario
                ElementoProduccion iterator = this.ContenidoProduccion [item];
                string s = iterator.Get_llave();//la llave que devuelve es el <id> en minusculas
                if (!datos.ContainsValue(s))
                {
                    datos.Add(item, s);
                    subllaves.Add(s);//datos incertados si aparecen almenos 1 vez
                    intsubllaves.Add(item);
                }
                else
                {
                    llavesStrdelosRepetidos.Add(s);
                    intLlavesDeLosRepetidos.Add(item);
                }
            }
            int c = 1;//lleva el conteo de la produccion
            //se calcula la siguiente produccion
            foreach (int item in llaves)
            {//volvemos a leer todas las producciones de la gramatica
                ElementoProduccion iterator = this.ContenidoProduccion [item];
                string s = iterator.Get_llave();//la llave que devuelve es el <id> en minusculas
                if (llavesStrdelosRepetidos.Contains(s))//si las tiene se procede a calcular como llave de los  repetidos
                {
                    string [] StrsRepetidos = llavesStrdelosRepetidos.ToArray();
                    int [] llaverepetidas = intLlavesDeLosRepetidos.ToArray();
                    int j = 0;
                    bool Inserto = false;
                    foreach (string strReps in StrsRepetidos)
                    {
                        if (strReps.CompareTo(s) == 0 && Inserto == false)
                        {

                            int V = this.Get_No_SiguienteProduccionRepetida(llaverepetidas [j], StrsRepetidos [j]);
                            if (iterator.Get_Cantidad_de_niveles() == 1)
                            {
                                RESPUESTA.Add(c, V);
                            }
                            else
                            { //cuando tiene mas de un nivel
                                char separador = Convert.ToChar(1333);
                                string [] r = iterator.Get_SiguieteProduccionPara_repetidos(c, separador, V).Split(separador);//en el arreglo la ultima posicion es la del nivel actual
                                int newnivel = r.Length - 1;//obtiene la posicion del nivel
                                int contador = 0;
                                c = Convert.ToInt32(r [newnivel]);
                                foreach (string ix in r)
                                {
                                    if (contador < r.Length && ix != "")
                                    {
                                        string [] s1 = r [contador].Split(' ');//primera pos es el nivel, siguiente es quien le toca
                                        if (s1.Length != 1)
                                        {
                                            RESPUESTA.Add(Convert.ToInt32(s1 [0]), Convert.ToInt32(s1 [1]));
                                        }
                                    }
                                    contador++;
                                }
                            }

                            llavesStrdelosRepetidos.Remove(StrsRepetidos [j]);//PARA QUE QUITE LOS REPETIDOS QUE YA UTILIZO
                            intLlavesDeLosRepetidos.Remove(llaverepetidas [j]);
                            Inserto = true;
                        }
                        j++;
                    }
                }
                else
                {
                    #region se calcula como llave de los no repetidos
                    if (iterator.Get_Cantidad_de_niveles() == 1)//si es una declarasion sin | y que no este repetida
                    {
                        RESPUESTA.Add(c, 0);
                    }
                    else
                    {//es cuando nopesee almenos otro nivel y no se repite
                        char separador = Convert.ToChar(1333);
                        string [] r = iterator.Get_SiguieteProduccionPara_No_repetidos(c, separador).Split(separador);//en el arreglo la ultima posicion es la del nivel actual
                        int newnivel = r.Length - 1;//obtiene la posicion del nivel
                        int contador = 0;
                        c = Convert.ToInt32(r [newnivel]);
                        foreach (string ix in r)
                        {
                            if (contador < r.Length && ix != "")
                            {
                                string [] s1 = r [contador].Split(' ');//primera pos es el nivel, siguiente es quien le toca
                                if (s1.Length != 1)
                                {
                                    RESPUESTA.Add(Convert.ToInt32(s1 [0]), Convert.ToInt32(s1 [1]));
                                }
                            }
                            contador++;
                        }
                    }
                    #endregion
                }
                c++;
            }
            return RESPUESTA;
        }
        #endregion

    }
    class ElementoProduccion
    {
        List<List<string>> contenido;
        string llave;//es la declarrasion <id>-> solo a la parte del <id>
        public ElementoProduccion(string key, List<List<string>> contenido)
        {
            this.llave = key;
            this.contenido = contenido;
        }
        public List<string> GetContenido_comoUNAUNIcaLista()
        {
            List<string> resp = new List<string>();

            foreach (var item in this.contenido)
            {//aquie se entra a la lista de listas
                List<string> ob = item.ToList<string>();
                foreach (string i in ob)
                {
                    resp.Add(i);
                }
            }
            return resp;
        }
        public string EnqNivelLoPosee(string s, int v)
        {
            foreach (var item in this.contenido)
            {//aquie se entra a la lista de listas
                List<string> ob = item.ToList<string>();
                v++;
            }
            return v.ToString();
        }
        public List<List<string>> Get_Contenindo()
        {
            return this.contenido;
        }
        public string Get_llave()
        {
            return this.llave;
        }
        /*aqui hacer el metodo get leng por producciones, llevando el conteo de las mismas*/

        public int Get_Cantidad_de_niveles()
        {//retorna la cantidad de listas que posee, cada valor entero representa la cantidad de | que lo conforman
            return this.contenido.Count;
        }

        public string Get_SiguieteProduccionPara_No_repetidos(int nivel, char separador)
        {
            int i = 1;
            string resp = "";
            foreach (var item in this.contenido)
            {//aquie se entra a la lista de listas
                if (i < contenido.Count)
                {
                    resp = resp + separador + nivel + " " + (nivel + 1).ToString();
                    nivel++;
                }
                else
                {
                    resp = resp + separador + nivel + " " + 0 + separador + nivel;
                    return resp;
                }
                i++;
            }
            return resp;
        }
        public string Get_SiguieteProduccionPara_repetidos(int nivel, char separador, int v)
        {
            int i = 1;
            string resp = "";
            foreach (var item in this.contenido)
            {//aquie se entra a la lista de listas
                if (i < contenido.Count)
                {
                    resp = resp + separador + nivel + " " + (nivel + 1).ToString();
                    nivel++;
                }
                else
                {
                    resp = resp + separador + nivel + " " + v + separador + nivel;
                    return resp;
                }
                i++;
            }
            return resp;
        }
        public int Calculador_de_nivelesV2(int v)
        {
            v = v - 1;//se le quita el calculo del nivel adicional
            foreach (var item in this.contenido)
            {//aquie se entra a la lista de listas
                v++;
            }
            return v;
        }
    }
}
