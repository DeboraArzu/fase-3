using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Fase_3_Compiladores__tablas_
{
    class Analisis
    {
        #region variables privadas de uso general
        private int pos, fil, col;
        private string texto;
        private const int A = 65;
        private const int a = 97;
        private const int Z = 90;
        private const int z = 122;
        private const int _0 = 48;
        private const int _9 = 57;
        private const int _ = 95;
        private string epsilon;
        bool start = false;
        #endregion
        //de uso especifico 
        private int cont_C;
        #region estructuras para alamcenar los datos
        List<string> palabras_definidas;
        List<string> includs;//almacenador de includs
        List<string> commentarios;
        listOperator operadores;//<key,value>, conjunto de operadores y precedencia left o right como su key
        Dictionary<string, List<string>> conjuntos;
        Dictionary<string, TOKEN> tokens;
        List<string> KEYWORDS;
        List<string> elementos_temps;//para el manejo de los elementos de los conjuntos y tokens
        List<string> ListaTodos;
        Dictionary<string, DatosTabla1> Simbolos;
        Dictionary<string, int> Keywordss = new Dictionary<string, int>();
        Dictionary<string, DatosTabla1> TokensMostrar = new Dictionary<string, DatosTabla1>();
        List<string> NoTerminal = new List<string>();
        List<List<string>> Terminales = new List<List<string>>();
        List<List<List<string>>> contenido = new List<List<List<string>>>();
        Dictionary<int, ElementoProduccion> Produccion = new Dictionary<int, ElementoProduccion>();
        #endregion

        public Analisis(string t)
        {
            this.epsilon = Convert.ToString("ԑ");
            this.commentarios = new List<string>();
            this.elementos_temps = new List<string>();
            this.operadores = new listOperator();
            this.conjuntos = new Dictionary<string, List<string>>();
            this.tokens = new Dictionary<string, TOKEN>();
            Simbolos = new Dictionary<string, DatosTabla1>();//primera tabla 
            this.palabras_definidas = new List<string>();
            this.KEYWORDS = new List<string>();
            this.ListaTodos = new List<string>();
            Produccion = new Dictionary<int, ElementoProduccion>();
            contenido = new List<List<List<string>>>();
            NoTerminal = new List<string>();
            Terminales = new List<List<string>>();
            this.palabras_definidas.Add("units");
            this.palabras_definidas.Add("tokens");
            this.palabras_definidas.Add("left");
            this.palabras_definidas.Add("keywords");
            this.palabras_definidas.Add("right");
            this.palabras_definidas.Add("comments");
            this.palabras_definidas.Add("to");
            this.includs = new List<string>();
            this.cont_C = 0;
            this.texto = t + "  ";
            this.pos = 0; this.fil = this.col = 1;
        }
        public Dictionary<int, ElementoProduccion> getProduccion()
        {
            return this.Produccion;
        }
        public Dictionary<string, DatosTabla1> getSimbolos()
        {
            return this.Simbolos;
        }
        void obtener_filyCol()
        {//manera momentanea de mostrar errores
            try
            {
                fil = col = 1;
                for (int j = 0; j < pos; j++)
                {
                    switch (this.texto[j])
                    {
                        case '\r':
                        case '\n':
                            fil++;
                            col = 1;
                            break;
                        default:
                            col++;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                fil = col = 1;
                throw;
            }
        }
        public string analizar()
        {
            string mensaje = "";
            try
            {
                mensaje = this.get_palabra_simple().ToUpper();//validacion de compiler
                int conta = 1;
                if (mensaje.CompareTo("COMPILER") != 0)
                {
                    this.obtener_filyCol();//manera momentanea de mostrar errores
                    return "No se definio la palabra COMPILER, error en al fila " + fil + " y columna " + col;
                }
                else
                {

                    mensaje = this.primer_char();//validacion del nombre del documento+<\s*>+<.>
                    if (mensaje == "" || !((mensaje[0] >= A && mensaje[0] <= Z) || (mensaje[0] >= a && mensaje[0] <= z) || mensaje[0] == _))
                    {
                        this.obtener_filyCol();
                        return "No se definio el nombre del <ARCHIVO COMPILER> (el ID) adecuadamente, se esperaba un ID error en al fila " + fil + " y columna " + col;
                    }
                    else
                    {
                        pos++;
                        mensaje = validador_de_IDS(mensaje, 0);//0 -> id(\s*).
                        if (mensaje.CompareTo("") != 0)
                        {
                            return mensaje;
                        }
                        else
                        {
                            pos++;
                            mensaje = get_palabra_simple().ToLower();
                            if (mensaje.CompareTo("units") == 0)//si biene la seccion de units
                            {//si en dado caso biene
                                mensaje = primer_char();
                                if (mensaje == "" || !((mensaje[0] >= A && mensaje[0] <= Z) || (mensaje[0] >= a && mensaje[0] <= z) || mensaje[0] == _))
                                {
                                    this.obtener_filyCol();
                                    return "Se esperaba un ID error en al fila " + fil + " y columna " + col;
                                }
                                else
                                {
                                    pos++;
                                    mensaje = this.validador_de_IDS(mensaje, 1);//id , id | id.
                                    if (mensaje.CompareTo("") != 0)
                                    {
                                        return mensaje;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!(this.includs.Count() == 0))//0 no vino seccion de includs 
                {
                    pos++;
                    mensaje = get_palabra_simple();
                }
                mensaje = mensaje.ToUpper();
                if (mensaje.CompareTo("UNITS.") == 0)//////////////////////////
                {
                    obtener_filyCol();
                    return "Se esperaba la definision de algun elemento (ID), para UNITS error en la fila " + fil + " columna " + col;
                }///////////////////////
                if (mensaje.CompareTo("TOKENS") != 0)//comienza la seccion de tokens
                {
                    obtener_filyCol();
                    return "Se esperaba la palabra TOKENS, error en la fila " + fil + " columna " + col;
                }//se valida que venga la plabra tokens + (algun espacio)+
                pos++;
                bool banderin = false;
                int mypeek = 0;
                bool vinoTokens = false, poseecheck = false, welcomeComments = false;
                int precedens = 1;
                #region uso de nuestro peeck
                while (this.texto.Length > pos && banderin == false)//para manejar y leer los tokens
                {
                    int prediccion = this.predecir_palabra();//ve que biene sin avanazar en la pos actual
                    if (prediccion == 0)
                    {
                        #region caso 0
                        obtener_filyCol();
                        return "Se esperaba ' o \" o un ID para definir un conjunto o token, error en la fila " + fil + " columna " + col;
                        #endregion
                    }
                    else if (prediccion == 1)
                    {
                        #region caso 1
                        obtener_filyCol();
                        return "No se puede utilizar RIGHT para definir un conjunto o token, error en la fila " + fil + " columna " + col;
                        #endregion
                    }
                    else if (prediccion == 2)
                    {
                        #region caso 2
                        obtener_filyCol();
                        return "No se puede utilizar LEFT para definir un conjunto o token, error en la fila " + fil + " columna " + col;
                        #endregion
                    }
                    else if (prediccion == 3)
                    {//solo biene el id
                        #region caso 3
                        mensaje = primer_char();
                        pos++;
                        mensaje = validador_de_IDS(mensaje, 2);
                        obtener_filyCol();
                        if (this.conjuntos.ContainsKey(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya se habia definido con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.includs.Contains(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya se habia definido en UNITS, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.palabras_definidas.Contains(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya que es una palabra reservada, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.tokens.ContainsKey(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + ">, ya se habia definido un token con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        return "Se esperaba definir un conjunto o token, se esperbaba un \"(\" o \"=\", error en la fila " + fil + " columna " + col;
                        #endregion
                    }
                    else if (prediccion == 4)
                    {//se biene una definision de conjunto id + (
                        #region caso 4
                        string nameconjunto = "";
                        mensaje = primer_char();
                        pos++;
                        mensaje = validador_de_IDS(mensaje, 2);
                        nameconjunto = mensaje;
                        //posibles errores no lexicos
                        if (this.conjuntos.ContainsKey(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya se habia definido con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.includs.Contains(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya se habia definido en UNITS, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.palabras_definidas.Contains(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya que es una palabra reservada, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.tokens.ContainsKey(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + ">, ya se habia definido un token con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        mensaje = this.primer_char();//se sabe que biene el (
                        pos++;//ya se paso el (
                        mensaje = this.validar_de_conjuntos(false);//porque aun no sabemos, empezamos la lectura
                        if (mensaje != "")
                        {
                            return mensaje;
                        }
                        string[] AXU = this.elementos_temps.ToArray();
                        this.conjuntos.Add(nameconjunto.ToLower(), AXU.ToList<string>());//se guardan los conjuntos con la llave lower para su post comparacion
                        ListaTodos.Add(nameconjunto.ToLower());

                        this.elementos_temps.Clear();//ya que se uso limpiar
                        #endregion     
                    }
                    else if (prediccion == 5)
                    {//id + = (manejo de los tokens) //cuando estoso comienzan bien
                        #region caso 5
                        string nameTokinio = "";
                        mensaje = primer_char();
                        pos++;
                        mensaje = validador_de_IDS(mensaje, 2);
                        nameTokinio = mensaje;
                        //posibles errores no lexicos
                        if (this.conjuntos.ContainsKey(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un token, ya que es un conjunto que ya se habia definido con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.includs.Contains(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un token, ya que es un UNTI que ya se habia definido en UNITS, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.palabras_definidas.Contains(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un token, ya que es una palabra reservada, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.tokens.ContainsKey(mensaje.ToLower()))
                        {
                            obtener_filyCol();
                            return "No se puede utilizar <" + mensaje.ToUpper() + ">, ya se habia definido ese token con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        mensaje = this.primer_char();//se sabe que biene el =
                        pos++;//ya se paso el = 
                        int tempinicio = pos;//desde donde inicia a leer si va a guardar
                        mensaje = this.validar_tokens(false, nameTokinio.ToLower());//ya valida muy bien 
                        if (mensaje != "")
                        {
                            return mensaje;
                        }//si esta bien escrito el pos esta en el punto <.>
                        string cadena = "";
                        for (int i = tempinicio; i < pos; i++)
                        {
                            cadena = cadena + this.texto[i];
                        }
                        string sucheck = "";
                        if (this.poseeCheck(cadena) == true)
                        {
                            poseecheck = true;
                            sucheck = "check";
                            Char[] l = cadena.ToCharArray();
                            int i = this.donde_comienzaCheck(cadena);
                            string sbtring = "";
                            for (int j = 0; j < i; j++)
                            {
                                sbtring = sbtring + l[j];
                            }
                            cadena = sbtring;
                        }
                        vinoTokens = true;
                        TOKEN tk = new TOKEN(nameTokinio, cadena, sucheck);
                        this.tokens.Add(nameTokinio, tk);
                        obtener_filyCol();
                        DatosTabla1 dt = new DatosTabla1(conta, 0, "");
                        Simbolos.Add(nameTokinio.ToLower(), dt);
                        TokensMostrar.Add(nameTokinio.ToLower(), dt);
                        ListaTodos.Add(nameTokinio.ToLower());
                        conta++;
                        this.elementos_temps.Clear();//ya que se uso limpiar
                        #endregion
                    }//en el caso 5, terminar de validar que no se envie "" como contnido del token
                    else if (prediccion == 6)
                    {//se espera un = o (
                        #region caso 6
                        mensaje = primer_char();
                        pos++;
                        mensaje = validador_de_IDS(mensaje, 2);
                        obtener_filyCol();
                        if (mensaje.ToLower().CompareTo("productions") == 0 && poseecheck == true)
                        {
                            return "Se esperaba la seccion de KEYWORDS, un token con check detectado con anterioridad error en la fila " + fil + " columna " + col;
                        }/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        if (this.conjuntos.ContainsKey(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya se habia definido con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.includs.Contains(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya se habia definido en UNITS, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.palabras_definidas.Contains(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + "> para definir un conjunto que ya que es una palabra reservada, error en la fila " + fil + " columna " + col;
                        }
                        else if (this.tokens.ContainsKey(mensaje.ToLower()))
                        {
                            return "No se puede utilizar <" + mensaje.ToUpper() + ">, ya se habia definido un token con anterioridad, error en la fila " + fil + " columna " + col;
                        }
                        return "Se esperaba definir un conjunto o token, se esperbaba un \"(\" o \"=\", error en la fila " + fil + " columna " + col;
                        #endregion
                    }
                    else if (prediccion == 7)
                    {//comienzan los operadores <'> o "
                        #region caso 7
                        this.elementos_temps.Clear();
                        mensaje = primer_char();//se obtiene el "  o el ' inicial
                        int ini = pos;//desde donde inicia
                        pos++;
                        bool b = false;
                        if (mensaje.CompareTo("\"") == 0)
                        {
                            b = true;
                        }
                        mensaje = this.Validar_operadores(b);
                        string temporal = "";
                        for (int i = ini; i < pos; i++)
                        {
                            temporal = temporal + texto[i].ToString();
                        }
                        Operators op;
                        string pp = "";
                        if (this.poseeLeft(temporal))
                        {
                            op = new Operators("left");
                            pp = "left";
                        }
                        else if (this.poseeRight(temporal))
                        {
                            op = new Operators("right");
                            pp = "right";
                        }
                        else
                        {
                            op = new Operators("right");
                            pp = "null";
                        }
                        List<string> elements = this.elementos_temps.ToList<string>();
                        this.operadores.push(elements, op.lado);
                        foreach (string sim in elements)
                        {
                            obtener_filyCol();
                            if (pp != "")
                            {
                                DatosTabla1 dt = new DatosTabla1(conta, precedens, pp);
                                Simbolos.Add(sim.ToLower(), dt);
                                TokensMostrar.Add(sim.ToLower(), dt);
                                ListaTodos.Add(sim.ToLower());
                            }
                            conta++;
                        }
                        this.elementos_temps.Clear();
                        precedens++;
                        #endregion
                    }
                    else if (prediccion == 8 && vinoTokens == true)//cambiamos de seccion en el archivo
                    {//keywords
                        pos--;
                        banderin = true;
                        mypeek = 8;
                    }
                    else if (prediccion == 8 && vinoTokens == false)//cambiamos de seccion en el archivo
                    {//keywords
                        obtener_filyCol();
                        return "Se esperaba la seccion de TOKENS antes que la de COMMENTS, error fila " + fil + " columna " + col;
                    }
                    else if (prediccion == 9 && vinoTokens == true)
                    {//comments
                        welcomeComments = true;
                        pos--;
                        banderin = true;
                        mypeek = 9;
                    }
                    else if (prediccion == 9 && vinoTokens == false)
                    {//comments
                        welcomeComments = true;
                        obtener_filyCol();
                        return "Se esperaba la seccion de TOKENS antes que la de COMMENTS, error fila " + fil + " columna " + col;
                    }
                    else if (prediccion == 10 && poseecheck == true)
                    {
                        obtener_filyCol();
                        return "Se esperaba la seccion de KEYWORDS, un token con check detectado con anterioridad error en la fila " + fil + " columna " + col;
                    }
                    else if (prediccion == 10 && poseecheck == false)
                    {
                        pos--;
                        mypeek = 10;
                        banderin = true;
                    }
                    pos++;
                }
                if (vinoTokens == false)
                {
                    obtener_filyCol();
                    return "Se esperaba la seccion de TOKENS, error fila " + fil + " columna " + col;
                }
                if (vinoTokens == true && poseecheck == true && mypeek == 9)
                {
                    obtener_filyCol();
                    return "Se esperaba la seccion de KEYWORDS, error fila " + fil + " columna " + col;
                }
                #endregion
                #region caso de producciones anticipado
                if (mypeek == 10)// si de una se viene el productions, 
                {
                    mensaje = primer_char();
                    if (mensaje.ToLower().CompareTo("p") != 0)
                    {
                        obtener_filyCol();
                        return "Se esperaba PRODUCTIONS, error fila " + fil + " columna " + col;
                    }
                    pos++;
                    mensaje = validador_de_IDS(mensaje, 2);
                    if (mensaje.ToLower().CompareTo("productions") != 0)
                    {
                        obtener_filyCol();
                        return "Se esperaba PRODUCTIONS, error fila " + fil + " columna " + col;
                    }
                    int ax11 = pos;//inicia despues de la productions 
                    mensaje = validar_producciones();
                    if (mensaje != "")
                    {
                        return mensaje;
                    }
                    int k = 0;
                    ElementoProduccion elpTemp;
                    foreach (string noter in NoTerminal)
                    {
                        elpTemp = new ElementoProduccion(noter, contenido[k]);
                        Produccion.Add(k + 1, elpTemp);
                        k++;
                    }

                    string AUXcadena = "";//contiene despues de producctions

                    for (int i = ax11; i < pos; i++)
                    {
                        AUXcadena = AUXcadena + this.texto[i].ToString();
                    }
                    if (this.poseeStart_Flecha(AUXcadena) == false)
                    {
                        int fi = 1, co = 1;
                        for (int j = 0; j < ax11; j++)
                        {
                            switch (this.texto[j])
                            {
                                case '\r':
                                case '\n':
                                    fi++;
                                    col = 1;
                                    break;
                                default:
                                    co++;
                                    break;
                            }
                        }
                        return "El bloque de PRODUCTIONS, no posee <START> -> <PRODUCCION>, puede cambiarlo desde la fila " + fi;
                    }
                    else
                        return "";

                }
                #endregion
                #region validar keywords
                if (mypeek == 8 && poseecheck == false)
                {
                    obtener_filyCol();
                    return "La seccion de KEYWORDS, no es permitida, debido a que ningun token anterior posee CHECK, error en la fila " + fil + " columna " + col;
                }
                if (mypeek == 8 && poseecheck == true)//seccion de keywords
                {
                    mensaje = primer_char();//estamos en la k
                    pos++;
                    mensaje = this.validador_de_IDS(mensaje, 2);
                    if (!(this.texto[pos] == ' ' || this.texto[pos] == '\t' || this.texto[pos] == '\n' || this.texto[pos] == '\r'))
                    {
                        obtener_filyCol();
                        return "Se esperaba un esparico, error en la fila " + fil + " columna " + col;
                    }
                    pos++;
                    this.elementos_temps.Clear();
                    mensaje = primer_char();
                    if (mensaje.CompareTo("'") == 0)
                    {
                        pos++;
                        mensaje = this.Validar_keywords(false);
                        if (mensaje != "")
                        {
                            return mensaje;
                        }
                        this.KEYWORDS = this.elementos_temps.ToList<string>();
                        this.elementos_temps.Clear();
                    }
                    else if (mensaje.CompareTo("\"") == 0)
                    {
                        pos++;
                        mensaje = this.Validar_keywords(true);
                        if (mensaje != "")
                        {
                            return mensaje;
                        }
                        this.KEYWORDS = this.elementos_temps.ToList<string>();
                        this.elementos_temps.Clear();
                    }
                    else
                    {
                        obtener_filyCol();
                        return "Se esperaba un ' o \" error en la fila " + fil + " columna " + col;
                    }

                }

                #endregion
                //aqui comenzar a avalidar la seccion de coments
                pos++;//Asi se cambia del .
                if (mensaje != "")// al retornar cualquiera de los dos, se estan en pos en la posicion del <.>
                {
                    return mensaje;
                }
                pos++;//agregar si no lo hace ANA
                int VendraComs = this.predecir_palabra();
                if (VendraComs == 9)
                {
                    welcomeComments = true;
                }
                if (welcomeComments == true)
                {
                    mensaje = this.validar_InicioComments();
                }

                if (mensaje != "")
                {
                    return mensaje;
                }
                //parte para empezar a analizar lexicamente producciones
                mensaje = primer_char();
                if (mensaje.ToLower().CompareTo("p") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba PRODUCTIONS, error fila " + fil + " columna " + col;
                }
                pos++;
                mensaje = validador_de_IDS(mensaje, 2);
                if (mensaje.ToLower().CompareTo("productions") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba PRODUCTIONS, error fila " + fil + " columna " + col;
                }
                int ax1 = pos;//ax1 contiene el valor despues de productions
                mensaje = validar_producciones();
                if (mensaje != "")
                {
                    return mensaje;
                }
                int g = 0;
                ElementoProduccion ElpTemp;
                foreach (string noter in NoTerminal)
                {
                    ElpTemp = new ElementoProduccion(noter, contenido[g]);
                    Produccion.Add(g + 1, ElpTemp);
                    g++;
                }
                string AUXcadena1 = "";//que es el que contiene todo despues de productions
                for (int i = ax1; i < pos; i++)
                {
                    AUXcadena1 = AUXcadena1 + this.texto[i].ToString();
                }
                if (this.poseeStart_Flecha(AUXcadena1) == false)
                {
                    int fi = 1, co = 1;
                    for (int j = 0; j < ax1; j++)
                    {
                        switch (this.texto[j])
                        {
                            case '\r':
                            case '\n':
                                fi++;
                                col = 1;
                                break;
                            default:
                                co++;
                                break;
                        }
                    }
                    return "El bloque de PRODUCTIONS, no posee <START> -> <PRODUCCION>, puede cambiarlo desde la fila " + fi;
                }
                else
                    return "";

            }
            catch (Exception)
            {
                throw;
            }
        }
        bool poseeStart_Flecha(string s)
        {
            try
            {
                int value = -1;
                Regex r = new Regex(@"(\s*)(\s*)(S|s)(T|t)(A|a)(R|r)(T|t)(\s*)(\s*)(=)(\s*)");
                var v = r.Match(s);
                value = v.Index;
                bool b = false;
                while (v.Success && b == false)
                {
                    b = true;
                }
                if (value >= 0 && b == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        #region Metodos auxiliares para tokens
        bool poseeCheck(string s)
        {
            try
            {
                int value = -1;
                Regex r = new Regex(@"(\s*)(C|c)(h|H)(E|e)(c|C)(K|k)(\s*)");
                var v = r.Match(s);
                value = v.Index;
                bool b = false;
                while (v.Success && b == false)
                {
                    b = true;
                }
                if (value >= 0 && b == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        int donde_comienzaCheck(string s)
        {
            try
            {
                int value = -1;
                Regex r = new Regex(@"(\s*)(C|c)(h|H)(E|e)(c|C)(K|k)(\s*)");
                var v = r.Match(s);
                value = v.Index;
                bool b = false;
                while (v.Success && b == false)
                {
                    b = true;
                }
                if (value >= 0 && b == true)
                {
                    return value;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
                throw;
            }
        }
        #endregion
        #region para validar si posee left o right
        bool poseeLeft(string s)
        {
            try
            {
                int value = -1;
                Regex r = new Regex(@"(\s*)(L|l)(E|e)(f|F)(t|T)(\s*)");
                var v = r.Match(s);
                value = v.Index;
                bool b = false;
                while (v.Success && b == false)
                {
                    b = true;
                }
                if (value >= 0 && b == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        bool poseeRight(string s)
        {
            try
            {
                int value = -1;
                Regex r = new Regex(@"(\s*)(R|r)(I|i)(G|g)(H|h)(T|t)(\s*)");
                var v = r.Match(s);
                value = v.Index;
                bool b = false;
                while (v.Success && b == false)
                {
                    b = true;
                }
                if (value >= 0 && b == true)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }
        #endregion
        //metodo de uso general para obtner palabar de 
        //la forma <\s>*<palabra><\s>+
        string get_palabra_simple()
        {
            string resp = "";
            try
            {
                while (this.texto.Length > pos && (this.texto[pos] == ' ' || this.texto[pos] == '\n' || this.texto[pos] == '\r' || this.texto[pos] == '\t'))
                {//ignorador de espacios
                    pos++;
                }
                while (this.texto.Length > pos && this.texto[pos] != ' ' && this.texto[pos] != '\n' && this.texto[pos] != '\r' && this.texto[pos] != '\t')
                {//formador de la plabra
                    resp += texto[pos].ToString();
                    pos++;
                }
                return resp;
            }
            catch (Exception)
            {
                return resp;
                throw;
            }
        }
        string primer_char()
        {
            string chr = "";
            try
            {
                while (this.texto.Length > pos && (this.texto[pos] == ' ' || this.texto[pos] == '\n' || this.texto[pos] == '\r' || this.texto[pos] == '\t'))
                {//ignorador de espacios
                    pos++;
                }
                return this.texto[pos].ToString();
            }
            catch (Exception)
            {
                return chr;
                throw;
            }
        }
        //para el manejo de validar ID + (espacios)* + (algun carcter de inicio o fin como el "." o el "(")*
        string validador_de_IDS(string s, int val)
        {
            string resp = "";
            try
            {
                switch (val)
                {
                    case 0://cuando es un id + (\s*)+(.)
                        while (pos < texto.Length && ((this.texto[pos] >= A && this.texto[pos] <= Z) || (this.texto[pos] >= a && this.texto[pos] <= z) || (this.texto[pos] >= _0 && this.texto[pos] <= _9) || this.texto[pos] == _))
                        {//para terminar de reconocer el nombre del doc, para al encontrar algo diferente a un ID
                            s = s + this.texto[pos];
                            pos++;
                        }
                        //ignorador de espacios
                        while (this.texto.Length > pos && (this.texto[pos] == ' ' || this.texto[pos] == '\n' || this.texto[pos] == '\r' || this.texto[pos] == '\t'))
                        {//ignorador de espacios
                            pos++;
                        }
                        resp = primer_char();
                        if (resp.CompareTo(".") != 0)
                        {
                            obtener_filyCol();
                            return "Se esperaba un . error en la fila " + fil + " y columna " + col;
                        }
                        else
                            this.palabras_definidas.Add(s.ToLower());
                        return "";
                    case 1:
                        while (pos < texto.Length && ((this.texto[pos] >= A && this.texto[pos] <= Z) || (this.texto[pos] >= a && this.texto[pos] <= z) || (this.texto[pos] >= _0 && this.texto[pos] <= _9) || this.texto[pos] == _))
                        {//para terminar de reconocer el id, para al encontrar algo diferente a un ID
                            s = s + this.texto[pos];
                            pos++;
                        }
                        string temp = s.ToLower();
                        if ((this.palabras_definidas.Contains(temp) && s.CompareTo("") != 0) || (this.includs.Contains(temp)))
                        {
                            obtener_filyCol();
                            if (this.palabras_definidas.Contains(temp) && s.CompareTo("") != 0)
                            {
                                return "No puede utilizar palabras reservadas para la estructura del archivo, para la seccion de units, error en la fila " + fil + " y columna " + col;
                            }//o ya definidas con aterioridad
                            else
                                return "No puede utilizar palabras ya definidas en seccion de units con anterioridad, para la seccion de units, error en la fila " + fil + " y columna " + col;
                        }
                        if (s.CompareTo("") == 0)
                        {
                            obtener_filyCol();
                            return "Se esperaba un id para la seccion de units, error en la fila " + fil + " y columna " + col;
                        }
                        this.includs.Add(temp.ToLower());
                        //pos++;
                        while (this.texto.Length > pos && (this.texto[pos] == ' ' || this.texto[pos] == '\n' || this.texto[pos] == '\r' || this.texto[pos] == '\t'))
                        {//ignorador de espacios
                            pos++;
                        }
                        temp = primer_char();
                        if (temp.CompareTo(".") == 0)
                        {
                            return "";
                        }
                        else if (temp.CompareTo(",") == 0)
                        {
                            pos++;
                            temp = primer_char();
                            if (!((temp[0] >= a && temp[0] <= z) || (temp[0] >= A && temp[0] <= Z) || temp[0] == _) || pos >= texto.Length || temp.CompareTo("") == 0)
                            {
                                obtener_filyCol();
                                return "Se esperaba un id para la seccion de units, error en la fila " + fil + " y columna " + col;
                            }
                            pos++;
                            return validador_de_IDS(temp, 1);
                        }
                        else
                            obtener_filyCol();
                        return "Se esperaba un . o una , para la seccion de units, error en la fila " + fil + " y columna " + col;
                    case 2://hay que retornar solo el id
                        while (pos < texto.Length && ((this.texto[pos] >= A && this.texto[pos] <= Z) || (this.texto[pos] >= a && this.texto[pos] <= z) || (this.texto[pos] >= _0 && this.texto[pos] <= _9) || this.texto[pos] == _))
                        {//para terminar de reconocer un ID
                            s = s + this.texto[pos];
                            pos++;
                        }
                        return s;
                    case 3://hay que retornar todo el operador para ', para el manejo de los operadores
                        while (pos < texto.Length && !(this.texto[pos] == ' ' || this.texto[pos] == '\n' || this.texto[pos] == '\t' || this.texto[pos] == '\r') && this.texto[pos].ToString().CompareTo("'") != 0)
                        {//para terminar de reconocer el operador
                            s = s + this.texto[pos];
                            pos++;
                        }
                        return s;
                    case 4://hay que retornar todo el operador para "
                        while (pos < texto.Length && !(this.texto[pos] == ' ' || this.texto[pos] == '\n' || this.texto[pos] == '\t' || this.texto[pos] == '\r') && this.texto[pos].ToString().CompareTo("\"") != 0)
                        {//para terminar de reconocer el operador
                            s = s + this.texto[pos];
                            pos++;
                        }
                        return s;
                    default:
                        break;
                }
                return resp;
            }
            catch (Exception)
            {
                if (val == 1)//si se acaba el archivo y lo ultimo que quedo es la , en el units
                {
                    obtener_filyCol();
                    return "Se esperaba un id para la seccion de units, error en la fila " + fil + " y columna " + col;
                }
                return resp;
                throw;
            }
        }
        //predice el comportamiento para la seccion de tokens
        int predecir_palabra()
        {//metodo para predecir el comportamiento de las secciones de tokens, simula un peek a conveniencia
            try
            {
                int x = pos;//se inicializa el auxiliar de la posicion
                string s = "";
                while (this.texto.Length > x && (this.texto[x] == ' ' || this.texto[x] == '\n' || this.texto[x] == '\t' || this.texto[x] == '\r'))
                {
                    x++;
                }
                if (this.texto.Length > x)
                {
                    s = this.texto[x].ToString();
                }
                else
                    return 0;//indica fin del archivo

                if ((this.texto[x] >= a && this.texto[x] <= z) || (this.texto[x] >= A && this.texto[x] <= Z))
                {//pareciera venir un id
                    x++;
                    while (this.texto.Length > x && ((this.texto[x] >= a && this.texto[x] <= z) || (this.texto[x] >= A && this.texto[x] <= Z) || (this.texto[x] >= _0 && this.texto[x] <= _9) || this.texto[x] == _))
                    {
                        s = s + this.texto[x].ToString();
                        x++;
                    }//otros ca
                    if (s.ToLower().CompareTo("keywords") == 0)
                    {//significa que hay un cambio de seccion
                        return 8;//significa que hay un cambio de seccion
                    }
                    if (s.ToLower().CompareTo("comments") == 0)
                    {//significa que hay un cambio de seccion
                        return 9;//significa que hay un cambio de seccion
                    }
                    if (s.ToLower().CompareTo("productions") == 0)
                    {
                        return 10;
                    }
                    if (s.ToUpper().CompareTo("RIGHT") == 0 || s.ToUpper().CompareTo("LEFT") == 0)
                    {
                        if (s.ToUpper().CompareTo("RIGHT") == 0)
                        {
                            return 1;//predice que biene right
                        }
                        else
                            return 2;//lefth
                    }
                    //encontremos el = o el (
                    while (this.texto.Length > x && (this.texto[x] == ' ' || this.texto[x] == '\n' || this.texto[x] == '\r' || this.texto[x] == '\t'))
                    {//ignorador de espacios
                        x++;
                    }
                    if (!(this.texto.Length > x))
                    {
                        return 3;//solo biene el id
                    }
                    if (this.texto[x].ToString().CompareTo("(") == 0)
                    {
                        return 4;//id + ( 
                    }
                    if (this.texto[x].ToString().CompareTo("=") == 0)
                    {
                        return 5;//id + = 
                    }

                    return 6;//se espera un = o (
                }
                else if (s.CompareTo("'") == 0 || s.CompareTo("\"") == 0)
                {
                    return 7;//comienzan los operadores
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
                throw;
            }
        }
        #region Manejador de Conjuntos
        string validar_de_conjuntos(bool validarRango)
        {//ver que valide el rango del elemento anterior
            string resp = "";
            try
            {//aqui usar la var de pos
                resp = primer_char();
                if (resp.CompareTo("\"") == 0 || resp.CompareTo("'") == 0)
                {
                    return resp = validar_comiaqAbre(resp, validarRango);//aqui ver como cuadrarlo    
                }
                else if (resp.ToLower().CompareTo("c") == 0)//ver que venga chr
                {
                    return resp = validar_CHR(validarRango);
                }
                else
                {
                    obtener_filyCol();
                    return "caracter invalido, deberia de venir ' o \" o chr, error en la fila " + fil + " y columna " + col;
                }
            }
            catch (Exception)
            {
                return resp;
                throw;
            }
        }
        string validar_comiaqAbre(string s, bool validarRango)
        {
            try
            {
                if (s.CompareTo("\"") == 0 || s.CompareTo("'") == 0)
                {
                    pos++;
                    bool b = false;
                    if (s.CompareTo("\"") == 0)
                    {
                        b = true;//indica que comenzo con "
                    }
                    s = validar_elemetnto(b, validarRango);
                    return s;
                }
                else
                    obtener_filyCol();
                return "Se esperaba un ' o un \", error en la definision del conjunto, error en la fila " + fil + " columna " + col;
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se esperaba un ' o un \", error en la definision del conjunto, error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        string validar_elemetnto(bool b, bool validarRango)
        {//true -> " false '
            try
            {
                if (!(this.texto.Length > pos))
                {
                    obtener_filyCol();
                    return "Se esperaba un CHAR, error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                }
                string s = this.texto[pos].ToString();
                if ((b == true && s.CompareTo("\"") == 0))
                {
                    obtener_filyCol();
                    return "Se esperaba un CHAR diferente a \", error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                }
                else if (b == false && s.CompareTo("'") == 0)
                {
                    obtener_filyCol();
                    return "Se esperaba un CHAR diferente a ', error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                }
                else if ((s[0] == ' ' || s[0] == '\n' || s[0] == '\t' || s[0] == '\r') && b == true)
                {
                    obtener_filyCol();
                    return "Se esperaba un CHAR seguido del \" no un TIPO DE ESPACIO, error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                }
                else if ((s[0] == ' ' || s[0] == '\n' || s[0] == '\t' || s[0] == '\r') && b == false)
                {
                    obtener_filyCol();
                    return "Se esperaba un CHAR seguido del ' no un TIPO DE ESPACIO, error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                }
                else
                {
                    string char1 = s;//contiene el elemento
                    pos++;
                    string ch = this.texto[pos].ToString();//aqui se valida que termine <char><'|">
                    if ((ch == "" && b == true) || (ch.CompareTo("'") == 0 && b == true) || ((ch[0] == ' ' || ch[0] == '\n' || ch[0] == '\t' || ch[0] == '\r') && b == true))
                    {
                        obtener_filyCol();
                        return "Se esperaba un \", error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                    }
                    else if ((ch == "" && b == false) || (ch.CompareTo("\"") == 0 && b == false) || ((ch[0] == ' ' || ch[0] == '\n' || ch[0] == '\t' || ch[0] == '\r') && b == false))
                    {
                        obtener_filyCol();
                        return "Se esperaba un ', error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                    }//si pasa eso es porque esta bien escrito
                    #region CODIGO PARA VALIDAR LOS RANGOS DE UN CONJUNTO
                    int TEMPORAL = 0;
                    if (validarRango == true)//por que esa accion se realiza aqui adentro del if, para que no lo haga otra vez (error logio ya validado), amenos q sea de nuevo necesario
                    {
                        TEMPORAL = 1;
                        char separador = Convert.ToChar(300);//tiene que ser un valor que no este en el rango de los conjuntos
                        string[] ar = this.elementos_temps.ToArray();
                        string[] araux = ar[this.elementos_temps.Count - 1].Split(separador);
                        int i = araux[araux.Length - 1][0];
                        int j = char1[0];
                        string x = ar[this.elementos_temps.Count - 1] + separador + char1;
                        if (i < j)
                        {
                            validarRango = false;
                            ar[this.elementos_temps.Count - 1] = x;
                            this.elementos_temps.Clear();
                            this.elementos_temps = ar.ToList<string>();
                        }
                        else if (i == j)
                        {
                            obtener_filyCol();
                            return "Rango mal definido, esta colocando el mismo rango, error en la fila " + fil + " columna " + col;
                        }
                        else if (i > j)
                        {
                            obtener_filyCol();
                            return "Rango mal definido, esta colocando un elemento MAYOR antes de definir el rango, error en la fila " + fil + " columna " + col;
                        }
                        validarRango = false;
                    }
                    #endregion
                    if (TEMPORAL == 0)
                    {
                        this.elementos_temps.Add(char1);
                    }
                    pos++;
                    ch = this.primer_char();//ver si es 
                    if (ch.CompareTo(".") == 0 || ch.CompareTo("+") == 0 || ch.CompareTo(")") == 0)
                    {
                        if (ch.CompareTo(")") == 0)
                        {
                            return "";//finalizo el conjunto correctamente
                        }
                        else if (ch.CompareTo(".") == 0)
                        {
                            pos++;
                            ch = this.primer_char();
                            if (ch.CompareTo(".") == 0)//el segundo punto del ..
                            {
                                pos++;//cambio al nuevo char
                                return this.validar_de_conjuntos(true);
                            }
                            else
                            {
                                obtener_filyCol();
                                return "Se esperaba ., para <..>, error en la fila " + fil + " columna " + col;
                            }
                        }
                        else
                        {//cuendo es + 
                            pos++;
                            return this.validar_de_conjuntos(false);
                        }
                    }
                    else
                    {
                        obtener_filyCol();
                        return "Caracter invalido, se esperaba un . o un + o un ), error en la fila " + fil + " y columna " + col;
                    }
                }
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se esperaba un CHAR, error en la definision del ELEMENTO del conjunto, error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        string validar_CHR(bool validarRango)
        {
            try
            {
                string s = "";
                try
                {// s = c + h + r
                    s = this.texto[pos].ToString() + this.texto[pos + 1].ToString() + this.texto[pos + 2].ToString();
                }
                catch (Exception)
                {
                    s = "";
                    throw;
                }
                if (s.ToUpper().CompareTo("CHR") == 0)
                {//posicion actual c
                    pos++;//h
                    pos++;//r
                    pos++;//se suponer que estamos en (
                    s = this.primer_char();
                    if (s.CompareTo("(") == 0)//validamos
                    {
                        pos++;
                        s = this.validarNUM_CHR(validarRango);
                        return s;
                    }
                    else
                        obtener_filyCol();
                    return "se esperaba \" (\", para el CHR(, error en la fila " + fil + " columna " + col;
                }
                else
                    obtener_filyCol();
                return "Se esperaba CHR, error en la fila " + fil + " columna " + col;
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se esperaba CHR, error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        string validarNUM_CHR(bool validarRango)
        {
            try
            {
                if (!(this.texto.Length > pos))
                {
                    obtener_filyCol();
                    return "Se esperaba un NUMERO error en la fila " + fil + " columna " + col; ;
                }
                string v = "";
                while ((this.texto[pos] >= _0 && this.texto[pos] <= _9) && (this.texto.Length > pos))
                {
                    v = v + this.texto[pos].ToString();
                    pos++;
                }//apartir de aqui esta en la pos que es diferente al numero
                if (v == "")
                {
                    obtener_filyCol();
                    return "Se esperaba NUMERO, error en la fila " + fil + " columna " + col;
                }
                if ((this.texto.Length > pos) && this.texto[pos].ToString().CompareTo(")") == 0)
                {//se esta en la pos del ) que cierra el chr(numero)
                    int TEMPORAL = 0;
                    #region validar rango
                    if (validarRango == true)
                    {
                        TEMPORAL = 1;
                        char separador = Convert.ToChar(300);
                        string[] ar = this.elementos_temps.ToArray();
                        string[] araux = ar[this.elementos_temps.Count - 1].Split(separador);
                        int i = araux[araux.Length - 1][0];
                        int j = Convert.ToInt32(v);
                        char X = Convert.ToChar(j);
                        string x = ar[this.elementos_temps.Count - 1] + separador + X.ToString();
                        if (i < j)
                        {
                            validarRango = false;
                            ar[this.elementos_temps.Count - 1] = x;
                            this.elementos_temps.Clear();
                            this.elementos_temps = ar.ToList<string>();
                        }
                        else if (i == j)
                        {
                            obtener_filyCol();
                            return "Rango mal definido, esta colocando el mismo rango, error en la fila " + fil + " columna " + col;
                        }
                        else if (i > j)
                        {
                            obtener_filyCol();
                            return "Rango mal definido, esta colocando un elemento MAYOR antes de definir el rango, error en la fila " + fil + " columna " + col;
                        }
                        validarRango = false;
                    }
                    #endregion
                    char elchar = Convert.ToChar(Convert.ToInt32(v.ToString()));
                    if (TEMPORAL == 0)
                    {
                        this.elementos_temps.Add(elchar.ToString());
                    }
                    //this.elementos_temps.Add(elchar.ToString());
                    pos++;//se cambia de posicion
                    v = primer_char();
                    if (v.CompareTo(".") == 0 || v.CompareTo("+") == 0 || v.CompareTo(")") == 0)
                    {
                        if (v.CompareTo(")") == 0)
                        {
                            return "";//se finalizo el conjunto
                        }
                        else if (v.CompareTo(".") == 0)
                        {
                            pos++;
                            if (v.CompareTo(".") == 0)
                            {
                                pos++;//cambio al nuevo char
                                return validar_de_conjuntos(true);
                            }
                            else
                            {
                                obtener_filyCol();
                                return "se esperaba ., para <..>, error en la fila " + fil + " columna" + col;
                            }
                        }
                        else
                        {
                            pos++;
                            return this.validar_de_conjuntos(false);
                        }
                    }
                    else
                    {

                        obtener_filyCol();
                        return "se esperaba un . o un + o un ), caracter invalido, error en la fila " + fil + " columna " + col;
                    }
                }
                else
                {
                    obtener_filyCol();
                    return "Se esperaba un ), para cerrar el CHR(NUMERO), error en la fila " + fil + " columna " + col;
                }
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se esperaba NUMERO, error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        #endregion
        #region Manejador de Tokens
        string validar_tokens(bool validar_l, string nameTok)
        {
            try
            {//id (letra)(letra|digito)*, ', ", ( 
                string s = primer_char();

                if (s.CompareTo("'") == 0 || s.CompareTo("\"") == 0)
                {
                    bool b = true;
                    if (s.CompareTo("'") == 0)
                    {
                        b = false;
                    }
                    pos++;//ya se mueve uno al elemento
                    return TKvalidar_comiasIniciales(b, false, nameTok);
                }
                else if (s.CompareTo("(") == 0)
                {
                    cont_C++;
                    pos++;
                    return this.validar_tokens(validar_l, nameTok);
                }
                else if (s.CompareTo(")") == 0)
                {
                    if (cont_C - 1 >= 0)
                    {
                        cont_C--;
                        pos++;
                        return this.validar_tokens(validar_l, nameTok);
                    }
                    else
                        obtener_filyCol();
                    return "Se esperaba ( anestes del ), Error en la fila " + fil + " columna " + col;
                }
                else if (s.CompareTo("|") == 0)
                {
                    pos++;
                    return this.validar_tokens(true, nameTok);
                }
                else if ((s[0] >= a && s[0] <= z) || (s[0] >= A && s[0] <= Z))
                {//si lo que encontro parece ser un ID 
                    pos++;
                    s = this.validador_de_IDS(s, 2);
                    if (s.ToLower().CompareTo("check") == 0 && validar_l == true)
                    {
                        obtener_filyCol();
                        return "Se esperaba un elemento antes de definir el CHECK, debido al <|>, error en la fila " + fil + " columna " + col;
                    }
                    if (s.ToLower().CompareTo("check") == 0 && validar_l == false)
                    {
                        string S = primer_char();
                        if (S.CompareTo(".") == 0)
                        {
                            return "";
                        }
                        else
                            obtener_filyCol();
                        return "Se esperaba despues del check un . error en la fila " + fil + " columan " + col;
                    }
                    if (s.ToLower().CompareTo("left") == 0)
                    {
                        obtener_filyCol();
                        return "No puede utilizar LEFT en la definision de un conjunto, error en la fila " + fil + " columna " + col;
                    }
                    if (s.ToLower().CompareTo("right") == 0)
                    {
                        obtener_filyCol();
                        return "No puede utilizar RIGHT en la definision de un conjunto, error en la fila " + fil + " columna " + col;
                    }
                    if (s.ToLower().CompareTo(nameTok) == 0)
                    {
                        obtener_filyCol();
                        return "No puedes utilizar el token que se esta definiendo dentro de la definision del mismo, error en la fila " + fil + " columna " + col;
                    }
                    validar_l = false;
                    if (this.conjuntos.ContainsKey(s.ToLower()))
                    {//cuando pertenece a un conjunto
                        s = primer_char();
                        #region
                        if (s.CompareTo(".") == 0)
                        {
                            return validar_tokens(false, nameTok);
                        }
                        if (s.CompareTo("*") == 0 || s.CompareTo("+") == 0 || s.CompareTo("?") == 0)
                        {
                            pos++;
                            return validar_tokens(false, nameTok);
                        }
                        else if (s.CompareTo(")") == 0)
                        {
                            if (cont_C - 1 >= 0)
                            {
                                cont_C--;
                            }
                            else if (!(cont_C - 1 >= 0))
                            {
                                obtener_filyCol();
                                return "Error al cerrar el ) se esperaba un ( antes, error fila " + fil + " columna " + col;
                            }
                            pos++;
                            string t = primer_char();
                            if (t.CompareTo("*") == 0 || t.CompareTo("+") == 0 || t.CompareTo("?") == 0)
                            {
                                pos++;
                            }
                            return validar_tokens(validar_l, nameTok);
                        }
                        else if (s.CompareTo("(") == 0)
                        {
                            return validar_tokens(false, nameTok);
                        }
                        else if (s.CompareTo("|") == 0)
                        {
                            pos++;
                            return validar_tokens(true, nameTok);
                        }
                        else
                            return validar_tokens(false, nameTok);
                        #endregion
                    }
                    else if (this.includs.Contains(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "No se puede utilizar <" + s.ToUpper() + "> para definir un token, ya que es se habia definido en UNITS, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.palabras_definidas.Contains(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "No se puede utilizar <" + s.ToUpper() + "> para definir un token, ya que es una palabra reservada, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.tokens.ContainsKey(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "No se puede utilizar <" + s.ToUpper() + ">, ya se utilizo para definir un token con anterioridad, error en la fila " + fil + " columna " + col;
                    }
                    else
                        obtener_filyCol();
                    return "Conjunto no definido, error en la fila " + fil + "col " + col;
                }
                else if (s.CompareTo(".") == 0)
                {
                    if (validar_l == true)
                    {
                        obtener_filyCol();
                        return "Se espera un elemeto, error en la definision del token fila " + fil + " columna " + col;
                    }
                    else if (cont_C != 0)
                    {
                        obtener_filyCol();
                        return "Faltaron <(> y/o <)>, error en la definision del token" + nameTok.ToUpper() + " fila " + fil + " columna " + col;
                    }
                    else
                        return "";
                }
                else
                {
                    obtener_filyCol();
                    return "se esperaba un ( o un =, error en la fila " + fil + "columna" + col;
                }
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "se esperaba un ( o un =, error en la fila " + fil + "columna" + col;
                throw;
            }
        }
        string TKvalidar_comiasIniciales(bool b, bool validar_l, string nameTok)
        {//true es " false '
            try
            {
                string s = "";
                try
                {
                    s = this.texto[pos].ToString();
                }
                catch (Exception)
                {
                    s = "";
                    throw;
                }

                if ((s == "" || s[0] == '\n' || s[0] == ' ' || s[0] == '\r' || s[0] == '\t'))
                {
                    obtener_filyCol();
                    return "Se esperaba char, error al definir el token en la fila " + fil + " columna " + col;
                }
                else if ((s.CompareTo("\"") == 0) && b == true)
                {
                    obtener_filyCol();
                    return "Se esperaba char diferente al \", error al definir el token en la fila " + fil + " columna " + col;
                }
                else if ((s.CompareTo("'") == 0) && b == false)
                {
                    obtener_filyCol();
                    return "Se esperaba char diferente al ', error al definir el token en la fila " + fil + " columna " + col;
                }
                else
                {
                    string val = s;
                    pos++;//cambiamos a lo que cierra el char
                    if (this.texto.Length > pos)
                    {
                        s = this.texto[pos].ToString();

                        if (s.CompareTo("'") == 0 && b == false)
                        {
                            pos++;
                            s = primer_char();
                            if (s.CompareTo(")") == 0)
                            {
                                if (cont_C - 1 >= 0)
                                {
                                    cont_C--;
                                    pos++;
                                    string t = primer_char();
                                    if (t.CompareTo("*") == 0 || t.CompareTo("+") == 0 || t.CompareTo("?") == 0)
                                    {
                                        pos++;
                                    }
                                    return validar_tokens(false, nameTok);
                                }
                                else
                                {
                                    obtener_filyCol();
                                    return "Se esperaba un ( antes del ), error al definir token en fila " + fil + " columna " + col;
                                }
                            }
                            if (s.CompareTo("*") == 0 || s.CompareTo("|") == 0 || s.CompareTo("?") == 0 || s.CompareTo("+") == 0 || s.CompareTo("(") == 0 || s.CompareTo(")") == 0)
                            {
                                if (s.CompareTo("*") == 0 || s.CompareTo("?") == 0 || s.CompareTo("+") == 0)
                                {
                                    val = val + s;
                                    pos++;
                                }
                                else if (s.CompareTo("|") == 0)
                                {
                                    pos++;
                                    return this.validar_tokens(true, nameTok);
                                }
                                else if (s.CompareTo("(") == 0)
                                {
                                }
                                else if (s.CompareTo(")") == 0)
                                {
                                    if (cont_C - 1 >= 0)
                                    {
                                        cont_C--;
                                        pos++;
                                        string t = primer_char();
                                        if (t.CompareTo("*") == 0 || t.CompareTo("+") == 0 || t.CompareTo("?") == 0)
                                        {
                                            pos++;
                                        }
                                        return validar_tokens(false, nameTok);
                                    }
                                    else
                                    {
                                        obtener_filyCol();
                                        return "Se esperaba un ( antes del ), error al definir token en fila " + fil + " columna " + col;
                                    }

                                }
                                //this.elementos_temps.Add(val);
                                return this.validar_tokens(false, nameTok);
                            }
                            else if ((s[0] >= a && s[0] <= z) || (s[0] >= A && s[0] <= Z))
                            {
                                //this.elementos_temps.Add(val);
                                return this.validar_tokens(false, nameTok);
                            }
                            else if (s.CompareTo(".") == 0)
                            {
                                if (validar_l == true)
                                {
                                    obtener_filyCol();
                                    return "Falto sefinir mas elementos, error en la fila " + fil + " columna " + col;
                                }
                                if (cont_C == 0)
                                {
                                    return "";
                                }
                                else
                                    obtener_filyCol();
                                return "Fataron cerrar <(> <)>, error en fila " + fil + " columna " + col;
                            }
                            else
                                obtener_filyCol();
                            return "Caracter invalido, error en la definision de token, error en la fila " + fil + " columna " + col;
                        }
                        else if (s.CompareTo("\"") == 0 && b == true)
                        {
                            pos++;
                            return validar_tokens(false, nameTok);
                        }
                        else
                        {
                            obtener_filyCol();
                            if (b == false)
                            {
                                return "Se esperaba ', error al definir el elemento del token en la fila " + fil + " columna " + col;
                            }
                            else
                                return "Se esperaba \", error al definir el elemento del token en la fila " + fil + " columna " + col;
                        }
                    }
                    else
                    {
                        obtener_filyCol();
                        if (b == false)
                        {
                            return "Se esperaba ', error al definir el elemento del token en la fila " + fil + " columna " + col;
                        }
                        else
                            return "Se esperaba \", error al definir el elemento del token en la fila " + fil + " columna " + col;
                    }
                }
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se esperaba \" o ' o un Id, para comenzar la definision del conjunto";
                throw;
            }
        }
        #endregion
        string Validar_operadores(bool b)
        {//true " false '
            try
            {
                string s = this.texto[pos].ToString();
                string aso = "";
                if (s.CompareTo("\"") == 0)
                {
                    obtener_filyCol();
                    return "Elemento invalido para poderador, no puede utilizar el \", error en la fila " + fil + " columna " + col;
                }
                else if (s.CompareTo("'") == 0 && b == false)
                {
                    obtener_filyCol();
                    return "Elemento invalido para poderador, no puede utilizar el ', error en la fila " + fil + " columna " + col;
                }
                else if (s[0] == ' ' || s[0] == '\n' || s[0] == '\t' || s[0] == '\r')
                {
                    obtener_filyCol();
                    return "Los espacios son invalidos para ser un poderador, no los puede utilizar, error en la fila " + fil + " columna " + col;
                }//en el evaludar de ids, 3 es ' y 4 el "
                pos++;
                if (b == true)
                {
                    s = this.validador_de_IDS(s, 4);
                    if (this.elementos_temps.Contains(s) || this.operadores.containselemento(s))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.conjuntos.ContainsKey(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad como conjunto, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.tokens.ContainsKey(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad como token, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.palabras_definidas.Contains(s.ToLower()) || this.includs.Contains(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad como una palabra reservada del documento, error en la fila " + fil + " columna " + col;
                    }
                    this.elementos_temps.Add(s);
                }
                if (b == false)
                {
                    s = this.validador_de_IDS(s, 3);
                    if (this.elementos_temps.Contains(s) || this.operadores.containselemento(s))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.conjuntos.ContainsKey(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad como conjunto, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.tokens.ContainsKey(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad como token, error en la fila " + fil + " columna " + col;
                    }
                    else if (this.palabras_definidas.Contains(s.ToLower()) || this.includs.Contains(s.ToLower()))
                    {
                        obtener_filyCol();
                        return "El operador ya ha sido definido con anterioridad como una palabra reservada del documento, error en la fila " + fil + " columna " + col;
                    }
                    this.elementos_temps.Add(s);
                }
                s = this.texto[pos].ToString();//posible llave que cierra, true " false '
                if (s.CompareTo("\"") == 0 && b == false)
                {
                    obtener_filyCol();
                    return "Se esperaba ', error en la fila " + fil + " columna " + col;
                }
                else if (s.CompareTo("'") == 0 && b == true)
                {
                    obtener_filyCol();
                    return "Se esperaba el \", error en la fila " + fil + " columna " + col;
                }
                else if (s[0] == ' ' || s[0] == '\n' || s[0] == '\t' || s[0] == '\r')
                {
                    obtener_filyCol();
                    return "Los espacios son invalidos para ser un poderador, no los puede utilizar, error en la fila " + fil + " columna " + col;
                }
                pos++;
                s = this.primer_char();
                if (s.CompareTo(".") == 0)/////////////////////////////////////////
                {
                    return "";
                }/////////////////////////////////////////
                else if (s.CompareTo(",") == 0)
                {
                    pos++;//cambio
                    s = this.primer_char();
                    if (s.CompareTo("\"") == 0)
                    {
                        pos++;
                        return this.Validar_operadores(true);
                    }
                    else if (s.CompareTo("'") == 0)
                    {
                        pos++;
                        return this.Validar_operadores(false);
                    }
                    else
                    {
                        obtener_filyCol();
                        return "Se esperaba definicion de un 'OPERADOR', error en la fila " + fil + " columna " + col;
                    }
                }
                else if ((s[0] >= a && s[0] <= z) || (s[0] >= A && s[0] <= Z))
                {
                    pos++;
                    s = this.validador_de_IDS(s, 2);
                    int bandera = 0;
                    if (s.ToLower().CompareTo("left") == 0 || s.ToLower().CompareTo("right") == 0)
                    {
                        bandera = 1;
                        aso = s;
                        s = primer_char();
                    }
                    if (bandera == 0)
                    {
                        obtener_filyCol();
                        return "Se esperaba LEFT o RIGHT, error en al fila " + fil + " columna " + col;
                    }
                    if (s.CompareTo(".") == 0)
                    {
                        return "";
                    }
                    else
                        obtener_filyCol();
                    return "Se esperaba un . error en la fila " + fil + " columna " + col;
                }
                else
                {
                    obtener_filyCol();
                    return "caracter invalido para definir el operador, error en la fila " + fil + " columna " + col;
                }
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se esperaba 'OPERADOR' o \"OPERADOR\", error en la definision de el operador, error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        string Validar_keywords(bool b)//true " false '
        {
            try
            {
                if ((this.texto[pos] >= a && this.texto[pos] <= z) || (this.texto[pos] >= A && this.texto[pos] <= Z))
                {
                    string s = this.texto[pos].ToString();
                    pos++;
                    s = this.validador_de_IDS(s, 2);
                    if (!this.elementos_temps.Contains(s.ToLower()))
                    {
                        this.elementos_temps.Add(s.ToLower());
                        if (this.texto[pos].ToString().CompareTo("'") == 0 && b == false)
                        {
                            pos++;
                            s = primer_char();
                            if (s.CompareTo(",") == 0)
                            {
                                pos++;
                                s = primer_char();
                                if (s.CompareTo("'") == 0)
                                {
                                    pos++;
                                    return this.Validar_keywords(false);
                                }
                                else if (s.CompareTo("\"") == 0)
                                {
                                    pos++;
                                    return this.Validar_keywords(true);
                                }
                                else
                                {
                                    obtener_filyCol();
                                    return "caracter invalido error en la fila " + fil + " columna " + col;
                                }
                            }
                            else if (s.CompareTo(".") == 0)
                            {
                                return "";
                            }
                            else
                            {
                                obtener_filyCol();
                                return "caracter invalido error en la fila " + fil + " columna " + col;
                            }
                        }
                        else if (this.texto[pos].ToString().CompareTo("\"") == 0 && b == true)
                        {
                            pos++;
                            s = primer_char();
                            if (s.CompareTo(",") == 0)
                            {
                                pos++;
                                s = primer_char();
                                if (s.CompareTo("'") == 0)
                                {
                                    pos++;
                                    return this.Validar_keywords(false);
                                }
                                else if (s.CompareTo("\"") == 0)
                                {
                                    pos++;
                                    return this.Validar_keywords(true);
                                }
                                else
                                {
                                    obtener_filyCol();
                                    return "caracter invalido error en la fila " + fil + " columna " + col;
                                }
                            }
                            else if (s.CompareTo(".") == 0)
                            {
                                return "";
                            }
                            else
                            {
                                obtener_filyCol();
                                return "caracter invalido error en la fila " + fil + " columna " + col;
                            }
                        }
                        else
                        {
                            if (b == true)
                            {
                                obtener_filyCol();
                                return "Se eseperaba \" error en la fila " + fil + " columna " + col;
                            }
                            obtener_filyCol();
                            return "Se eseperaba ' error en la fila " + fil + " columna " + col;
                        }
                    }
                    else
                    {
                        obtener_filyCol();
                        return "KEYWORD ya definida anteriormente, error en la fila " + fil + " columna " + col;
                    }
                }
                else
                    obtener_filyCol();
                return "Se eseperaba un ID, error en la fila " + fil + " columna " + col;
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se eseperaba un ID, error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        string Validar_comentario(bool b)
        { //true " false '
            try
            {
                string s = this.texto[pos].ToString();
                if (b == true)
                {
                    pos++;
                    s = this.validador_de_IDS(s, 4);
                }
                else
                {
                    pos++;
                    s = this.validador_de_IDS(s, 3);
                }//luego se valida que no se utilizara anteriormente en algo
                if (this.conjuntos.ContainsKey(s.ToLower()))
                {
                    obtener_filyCol();
                    return "El elemento, ya fue definido para un conjunto antes, no se puede usar para comentarios, error fila " + fil + " columna " + col;
                }
                else if (this.includs.Contains(s.ToLower()))
                {
                    obtener_filyCol();
                    return "El elemento, ya fue definido para un UNTIS antes, no se puede usar para comentarios, error fila " + fil + " columna " + col;
                }
                else if (this.KEYWORDS.Contains(s.ToLower()))
                {
                    obtener_filyCol();
                    return "El elemento, ya fue definido para un KEYWORD antes, no se puede usar para comentarios, error fila " + fil + " columna " + col;
                }
                else if (this.operadores.containselemento(s))
                {
                    obtener_filyCol();
                    return "El elemento, ya fue definido para un operador antes, no se puede usar para comentarios, error fila " + fil + " columna " + col;
                }
                else if (this.palabras_definidas.Contains(s))
                {
                    obtener_filyCol();
                    return "El elemento, es una palabra reservada, no se puede usar para comentarios, error fila " + fil + " columna " + col;
                }
                else if (this.tokens.ContainsKey(s.ToLower()))
                {
                    obtener_filyCol();
                    return "El elemento es un TOKEN, no se puede usar para comentarios, error fila " + fil + " columna " + col;
                }//luego validar si termina bien
                if (b == true && s.CompareTo("'") == 0)
                {
                    obtener_filyCol();
                    return "se esperaba \" error en la fila " + fil + " columna " + col;
                }
                if (b == false && s.CompareTo("\"") == 0)
                {
                    obtener_filyCol();
                    return "se esperaba ' error en la fila " + fil + " columna " + col;
                }
                this.commentarios.Add(s.ToLower());
                pos++;
                s = primer_char();
                if (s.CompareTo(".") == 0)
                {
                    return "";
                }
                else if (s.ToLower().CompareTo("t") == 0)
                {
                    pos++;
                    s = this.validador_de_IDS(s, 2);
                    if (s.ToLower().CompareTo("to") == 0)
                    {
                        s = primer_char();
                        if (s.CompareTo("'") == 0)
                        {
                            pos++;
                            return this.Validar_comentario(false);
                        }
                        else
                            pos++;
                        return this.Validar_comentario(true);
                    }
                    else
                        obtener_filyCol();
                    return "Se esperaba la palabra TO, error en la fila " + fil + " columna " + col;
                }
                else
                {
                    if (s.ToLower().CompareTo("c") == 0)
                    {
                        string comentario = "";
                        while (s.ToLower().CompareTo(".") != 0)
                        {
                            comentario += s;
                            pos++;
                            s = primer_char();
                        }
                        if (comentario.ToLower().CompareTo("comentario") != 0)
                        {
                            return "Se esperaba que viniera la palabra comentario fila " + fil + "columna " + col;
                        }
                        return "";
                    }
                }
                obtener_filyCol();
                return "Se esperaba la palabra TO o <.> error en la fila " + fil + " columna " + col;
            }
            catch (Exception)
            {
                obtener_filyCol();
                return "Se eseperaba un '<EXPRESION PARA COMENTARIO>' o \"<EXPRESION PARA COMENTARIO>\", error en la fila " + fil + " columna " + col;
                throw;
            }
        }
        //validar producciones version jorge
        string validar_producciones()
        {//valida el <comienzo de una produccion > ->
            try
            {
                //validar start = ....
                string resp = "";
                start = false;
                resp = primer_char();
                if (resp.CompareTo("<") != 0 && resp.ToLower().CompareTo("s") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba < error en la fila " + fil + " columna " + col;
                }
                //START
                if (resp.ToLower().CompareTo("s") == 0)
                {
                    start = true;
                }
                pos++;
                resp = primer_char();
                if (!((resp[0] >= A && resp[0] <= Z) || (resp[0] >= a && resp[0] <= z)))
                {
                    obtener_filyCol();
                    return "Se esperaba un ID, error fila " + fil + " columna " + col;
                }
                pos++;
                resp = validador_de_IDS(resp, 2);
                NoTerminal.Add("<" + resp.ToLower() + ">");
                resp = primer_char();
                if (resp.CompareTo(">") != 0 && resp.CompareTo("=") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba > error en la fila " + fil + " columna " + col;
                }
                pos++;
                resp = primer_char();
                pos++;
                try
                {
                    resp = resp + this.texto[pos];
                }
                catch (Exception)
                {
                    resp = resp + " ";
                    throw;
                }
                if (resp.CompareTo("->") != 0)
                {
                    pos--;
                    obtener_filyCol();
                    if (start == true)
                    {
                        pos--;
                        pos--;
                        resp = primer_char();
                    }
                    else
                    {
                        return "Se esperaba -> error en la fila " + fil + " columna " + col;
                    }
                }
                pos++;
                Terminales.Add(new List<string>());
                resp = validar_contenido_produccion(false);
                contenido.Add(Terminales);
                Terminales = new List<List<string>>();
                mas = 0;
                if (resp != "")
                {
                    return resp;
                }
                pos++;
                resp = primer_char();
                if (resp.CompareTo("<") == 0 || resp.ToLower().CompareTo("s") == 0)
                {
                    return this.validar_producciones();
                }
                else if (resp == "" || resp == " ")
                {
                    return "";
                }
                else
                    obtener_filyCol();
                return "Caracter invalido para iniciar producciones, error en fila " + fil + " columna " + col;

            }
            catch (Exception) { obtener_filyCol(); return "Mala declaracion en la fila " + fil + " columna " + col; }
        }
        static int mas = 0;
        //string int mas2 = 0;
        string validar_contenido_produccion(bool b)
        {//BOOL, es para validar que biene algo valido antes de comenzar {, true si, false no
            string resp = primer_char();
            //validar que no sea epsilon, si es ignorar
            #region Validar Epsilon
            if (resp[0].ToString().CompareTo(epsilon) == 0)
            {
                Terminales[mas].Add(epsilon);
                pos++;
                return validar_contenido_produccion(true);
            }
            #endregion
            #region Validar contenido or
            else if (resp.CompareTo("|") == 0)
            {
                Terminales.Add(new List<string>());
                pos++;
                resp = primer_char();
                if (resp.CompareTo("|") == 0)
                {
                    obtener_filyCol();
                    return "No se esperaba |, sino una DEFINISION DE UNA PRODUCCION, error en la fila " + fil + " columna " + col;
                }
                else if (resp.CompareTo(".") == 0)
                {
                    obtener_filyCol();
                    return "No se esperaba <.> sino una DEFINISION DE UNA PRODUCCION, error en la fila " + fil + " columna " + col;
                }
                mas++;
                return this.validar_contenido_produccion(false);
            }
            else if (resp.CompareTo(".") == 0)
            {
                return "";
            }
            #endregion
            #region Vaidar contenido '
            else if (resp.CompareTo("'") == 0)
            {
                pos++;
                if (this.texto[pos] == '\n' || this.texto[pos] == '\t' || this.texto[pos] == '\r' || this.texto[pos] == ' ')
                {
                    obtener_filyCol();
                    return "Se esperaba un elemeto, no un espacio, error fila " + fil + " columan " + col;
                }
                else if (this.texto[pos].ToString().CompareTo("'") == 0)
                {
                    obtener_filyCol();
                    return "Se esperaba un elemento distinto de ' error fila " + fil + " columan " + col;
                }
                resp = this.texto[pos].ToString();
                pos++;
                resp = validador_de_IDS(resp, 3);
                if (this.texto[pos].ToString().CompareTo("'") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba un ' error fila " + fil + " columan " + col;
                }
                Terminales[mas].Add(resp.ToLower());
                if (!ListaTodos.Contains(resp.ToLower()))
                {
                    string[] llavesitas = TokensMostrar.Keys.ToArray();
                    int nuevaLlave = -1;
                    foreach (string k in llavesitas)
                    {
                        DatosTabla1 iterador = TokensMostrar[k];
                        if (nuevaLlave < iterador.numtoken)
                        {
                            nuevaLlave = iterador.numtoken;
                        }
                    }
                    nuevaLlave++;
                    DatosTabla1 t = new DatosTabla1(nuevaLlave, 0, "");
                    TokensMostrar.Add(resp.ToLower(), t);
                    Simbolos.Add(resp.ToLower(), t);
                    ListaTodos.Add(resp.ToLower());
                }
                pos++;
                resp = primer_char();
                if (resp.CompareTo(".") == 0)
                {
                    return "";
                }
                return this.validar_contenido_produccion(true);
            }
            #endregion
            #region validar contenido "
            else if (resp.CompareTo("\"") == 0)
            {
                pos++;
                if (this.texto[pos] == '\n' || this.texto[pos] == '\t' || this.texto[pos] == '\r' || this.texto[pos] == ' ')
                {
                    obtener_filyCol();
                    return "Se esperaba un elemeto, no un espacio, error fila " + fil + " columan " + col;
                }
                else if (this.texto[pos].ToString().CompareTo("\"") == 0)
                {
                    obtener_filyCol();
                    return "Se esperaba un elemento distinto de \" error fila " + fil + " columan " + col;
                }
                resp = this.texto[pos].ToString();
                pos++;
                resp = validador_de_IDS(resp, 4);//todo menos "
                if (this.texto[pos].ToString().CompareTo("\"") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba un \" error fila " + fil + " columan " + col;
                }
                Terminales[mas].Add(resp.ToLower());
                if (!ListaTodos.Contains(resp.ToLower()))
                {
                    string[] llavesitas = TokensMostrar.Keys.ToArray();
                    int nuevaLlave = -1;
                    foreach (string k in llavesitas)
                    {
                        DatosTabla1 iterador = TokensMostrar[k];
                        if (nuevaLlave < iterador.numtoken)
                        {
                            nuevaLlave = iterador.numtoken;
                        }
                    }
                    nuevaLlave++;
                    DatosTabla1 t = new DatosTabla1(nuevaLlave, 0, "");
                    TokensMostrar.Add(resp.ToLower(), t);
                    Simbolos.Add(resp.ToLower(), t);
                    ListaTodos.Add(resp.ToLower());
                }
                pos++;
                resp = primer_char();
                if (resp.CompareTo(".") == 0)
                {
                    return "";
                }
                return this.validar_contenido_produccion(true);
            }
            #endregion
            #region Validar <id>
            else if (resp.CompareTo("<") == 0)
            {
                pos++;
                resp = primer_char();
                if (!((resp[0] >= A && resp[0] <= Z) || (resp[0] >= a && resp[0] <= z)))
                {
                    obtener_filyCol();
                    return "Se esperaba un ID, error fila " + fil + " columna " + col;
                }
                pos++;
                resp = validador_de_IDS(resp, 2);
                Terminales[mas].Add("<" + resp.ToLower() + ">");
                resp = primer_char();
                if (resp.CompareTo(">") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba > error en la fila " + fil + " columna " + col;
                }
                pos++;
                resp = primer_char();
                if (resp.CompareTo(".") == 0)
                {
                    return "";
                }
                return validar_contenido_produccion(true);
            }
            #endregion
            #region Validar ids
            else if ((resp[0] >= A && resp[0] <= Z) || (resp[0] >= a && resp[0] <= z))
            {
                pos++;
                resp = validador_de_IDS(resp, 2);
                if (!ListaTodos.Contains(resp.ToLower()))
                {
                    string[] llavesitas = TokensMostrar.Keys.ToArray();
                    int nuevaLlave = -1;
                    foreach (string k in llavesitas)
                    {
                        DatosTabla1 iterador = TokensMostrar[k];
                        if (nuevaLlave < iterador.numtoken)
                        {
                            nuevaLlave = iterador.numtoken;
                        }
                    }
                    nuevaLlave++;
                    DatosTabla1 t = new DatosTabla1(nuevaLlave, 0, "");
                    TokensMostrar.Add(resp.ToLower(), t);
                    Simbolos.Add(resp.ToLower(), t);
                    ListaTodos.Add(resp.ToLower());
                }
                Terminales[mas].Add(resp.ToLower());
                return this.validar_contenido_produccion(true);
            }
            #endregion
            #region validar acciones
            else if (resp.CompareTo("{") == 0 && b == true)
            {
                pos++;
                resp = primer_char();
                if (!((resp[0] >= A && resp[0] <= Z) || (resp[0] >= a && resp[0] <= z)))
                {
                    obtener_filyCol();
                    return "Se esperaba un id, error fila " + fil + " columna " + col;
                }
                pos++;
                resp = validador_de_IDS(resp, 2);
                Terminales[mas].Add("{" + resp.ToLower() + "}");
                resp = primer_char();
                if (resp.CompareTo("}") != 0)
                {
                    obtener_filyCol();
                    return "Se esperaba una }, error fila " + fil + " columna " + col;
                }
                pos++;
                return validar_contenido_produccion(false);
            }
            else if (resp.CompareTo("{") == 0 && b == false)
            {
                obtener_filyCol();
                return "Se esperaba definision de \"valor\" o 'valor' o <declarasion> o un ID o EPSILON, error en la fila " + fil + " columna " + col;
            }
            else
                obtener_filyCol();
            return "Caracter invalido, error en la fila " + fil + " columna " + col;
            #endregion
        }

        string validar_InicioComments()
        {
            string mensaje = primer_char();
            if (mensaje.ToLower().CompareTo("c") != 0)
            {
                obtener_filyCol();
                return "Se esperaba la palabra reservada COMMENTS, error en la fila " + fil + " columna " + col;
            }
            pos++;
            mensaje = this.validador_de_IDS(mensaje, 2);
            if (mensaje.ToLower().CompareTo("comments") != 0)
            {
                obtener_filyCol();
                return "Se esperaba la palabra reservada COMMENTS, error en la fila " + fil + " columna " + col;
            }
            mensaje = primer_char();
            if (mensaje.CompareTo("'") == 0 || mensaje.CompareTo("\"") == 0)
            {
                bool b = false;
                if (mensaje.CompareTo("\"") == 0)
                {
                    b = true;
                }
                pos++;//asi se empiza a leer desde el elemento, no la camia(s) que abre(n)
                mensaje = this.Validar_comentario(b);
            }

            if (mensaje != "")
            {
                return mensaje;
            }
            pos++;
            int v = predecir_palabra();
            if (v == 9)
            {
                return validar_InicioComments();
            }
            return mensaje;
        }

        public int posicion()
        {
            return pos;
        }
        public List<string> TABLA1()
        {
            List<string> prueba = new List<string>();
            foreach (string key in TokensMostrar.Keys)
            {
                int num = TokensMostrar[key].numtoken;
                int pre = TokensMostrar[key].precedencia;
                string aso = TokensMostrar[key].Asociatividad;
                prueba.Add(num + "‡" + key + "‡" + pre + "‡" + aso);
            }
            return prueba;
        }

        public List<string> TABLA_key()
        {
            List<string> prueba = new List<string>();
            foreach (string key in Keywordss.Keys)
            {
                int num = Keywordss[key];
                prueba.Add(num + "‡" + key);
            }
            return prueba;
        }
        public List<List<string>> getTerminales()
        {
            return this.Terminales;
        }
    }
}
