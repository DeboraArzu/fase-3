using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Fase_3_Compiladores__tablas_
{
    public partial class Form1 : Form
    {
        string texto = "";
        string mensaje = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cargarArchivoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog window = new OpenFileDialog();
            window.Title = "Cargar Archivo";
            window.ShowDialog();
            this.rchtb_Texto.Clear();
            try
            {
                if (window.FileName != "")
                {
                    string ruta = window.FileName.ToString();
                    string[] linea = System.IO.File.ReadAllLines(ruta);
                    string ln = "";
                    foreach (string line in linea)
                    {
                        ln = ln + line + "\n";
                    }
                    this.rchtb_Texto.Text = ln + "  ";
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Archivo no seleccionado");
                throw;
            }
        }

        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            texto = this.rchtb_Texto.Text;
            fix();
            List<string> tab = new List<string>();
            List<string> extra = new List<string>();
            rchtb_Texto.Select(0, rchtb_Texto.Text.Length);
            rchtb_Texto.SelectionBackColor = Color.Black;
            if (this.rchtb_Texto.Text != "")
            {
                Analisis gram = new Analisis(texto); //archivo de entrada
                mensaje = gram.analizar();
                if (mensaje != "")
                {
                    rchtb_Texto.Select(0, gram.posicion());
                    txtMensaje.SelectionBackColor = Color.Red;
                    this.TABLA1.Rows.Clear();
                    this.dtgvNoTerminalTable.Rows.Clear();
                    this.dgvProducciones.Rows.Clear();
                }
                else
                {
                    rchtb_Texto.Select(0, gram.posicion());
                    mensaje = "Archivo correcto.";
                    txtMensaje.SelectionBackColor = Color.Green;
                    TablaNo_Terminales Tabla2 = new TablaNo_Terminales(gram.getProduccion());
                    tab = gram.TABLA1();
                    llenar_tabla_1(tab);
                    //llenar_tabla_keywords(gramatica.TABLA_key());
                    this.llenar_tabla_NoTerminales(Tabla2.CalcularNoTerminales());
                    Tabla_Producciones Tabla3 = new Tabla_Producciones(Tabla2.getVaLores(), gram.getSimbolos(), Tabla2.getContenidoProduccion());
                    llenar_tabla_producciones(Tabla3.Elementos(), Tabla2.CalcularProducciones());
                }
                txtMensaje.Text = mensaje;

            }
            else
                MessageBox.Show("No hay texto para analizar");
        }

        public void llenar_tabla_1(List<string> datos)
        {
            this.TABLA1.Rows.Clear();
            this.TABLA1.Rows.Add(datos.Count);
            int fila = 0;
            List<string> da = new List<string>();
            foreach (string part in datos)
            {
                string[] diviciones = part.Split('‡');
                TABLA1.Rows[fila].Cells[0].Value = diviciones[0];
                TABLA1.Rows[fila].Cells[1].Value = diviciones[1];
                if (diviciones[2].Equals("0"))
                {
                    TABLA1.Rows[fila].Cells[2].Value = "";
                }
                else
                {
                    TABLA1.Rows[fila].Cells[2].Value = diviciones[2];
                }
                TABLA1.Rows[fila].Cells[3].Value = diviciones[3];
                fila++;
            }
        }

        public void llenar_tabla_NoTerminales(List<string> datos)
        {
            this.dtgvNoTerminalTable.Rows.Clear();
            this.dtgvNoTerminalTable.Rows.Add(datos.Count);
            int fila = 0;
            List<string> da = new List<string>();
            foreach (string part in datos)
            {
                string[] diviciones = part.Split('‡');
                dtgvNoTerminalTable.Rows[fila].Cells[0].Value = diviciones[0];
                dtgvNoTerminalTable.Rows[fila].Cells[1].Value = diviciones[1];
                dtgvNoTerminalTable.Rows[fila].Cells[2].Value = diviciones[2];
                dtgvNoTerminalTable.Rows[fila].Cells[3].Value = diviciones[3];
                fila++;
            }
        }

        public String ObtenerValores(List<int> x)
        {
            string aux = "";
            if (x.Count > 0)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    aux += (x[i]).ToString() + ",";
                }
                aux = aux.Substring(0, aux.Length - 1);
            }
            return aux;
        }

        public void llenar_tabla_producciones(List<List<int>> datos, Dictionary<int, int> pr)
        {
            dgvProducciones.Rows.Clear();
            if (datos != null)
            {
                dgvProducciones.Rows.Add(datos.Count);
                int fila = 0;
                foreach (List<int> s in datos)
                {
                    if (ObtenerValores(s) == "0")
                    {
                        dgvProducciones[0, fila].Value = fila + 1;
                        dgvProducciones[1, fila].Value = s.Count - 1;
                        dgvProducciones[2, fila].Value = pr[fila + 1];
                        dgvProducciones[3, fila].Value = "[" + ObtenerValores(s) + "]";

                    }
                    else
                    {
                        dgvProducciones[0, fila].Value = fila + 1;
                        dgvProducciones[1, fila].Value = s.Count;
                        dgvProducciones[2, fila].Value = pr[fila + 1];
                        dgvProducciones[3, fila].Value = "[" + ObtenerValores(s) + "]";
                    }
                    fila++;
                }
            }
        }

        public void escribir_Tablas(string ruta)
        {
            TextWriter sw = new StreamWriter(ruta);
            sw.WriteLine("TABLA DE OPERADORES");
            sw.WriteLine("   No.TOKEN   ‡         SIMBOLO            ‡  PRECEDENCIA  ‡  ASOCIATIVIDAD  ‡");
            for (int i = 0; i < TABLA1.Rows.Count - 1; i++)
            {
                for (int j = 0; j < TABLA1.Columns.Count; j++)
                {
                    sw.Write("\t" + TABLA1.Rows[i].Cells[j].Value.ToString() + "\t" + "‡");
                }
                sw.WriteLine("\n");
            }
            sw.WriteLine("_______________________________________________________________________________");
            sw.WriteLine("\n");
            sw.WriteLine("\n");
            sw.WriteLine("TABLA DE SIMBOLOS NO TERMINALES");
            sw.WriteLine("   NUMERO   ‡         SIMBOLO            ‡        FIRST       ‡  PRODUCCION  ‡");

            for (int i = 0; i < dtgvNoTerminalTable.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dtgvNoTerminalTable.Columns.Count; j++)
                {
                    sw.Write("\t" + dtgvNoTerminalTable.Rows[i].Cells[j].Value.ToString() + "\t" + "‡");
                }
                sw.WriteLine("\n");
            }

            sw.WriteLine("________________________________________________________________________________");
            sw.WriteLine("\n");
            sw.WriteLine("\n");
            sw.WriteLine("TABLA DE PRODUCCIONES");
            sw.WriteLine("   PRODUCCION   ‡    LONGITUD   ‡   SIGUIENTE   ‡  ELEMENTOS  ‡");
            for (int i = 0; i < dgvProducciones.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dgvProducciones.Columns.Count; j++)
                {
                    sw.Write("\t" + dgvProducciones.Rows[i].Cells[j].Value.ToString() + "\t" + "‡");
                }
                sw.WriteLine("\n");
            }
            sw.WriteLine("_________________________________________________________________");
            sw.Close();
        }

        private void convertirtokens()
        {
            string pattern = @"tokens";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(texto, replacement);

            Console.WriteLine("Original String: {0}", texto);
            Console.WriteLine("Replacement String: {0}", result);
            this.texto = "";
            this.texto = result;
            Console.WriteLine("Tokens eliminados: {0}", this.texto);
        }

        private void convertirsets()
        {
            string pattern = @"sets";
            string replacement = "tokens";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(texto, replacement);

            Console.WriteLine("Original String: {0}", texto);
            Console.WriteLine("Replacement String: {0}", result);
            texto = result;
        }

        private void conjuntos(string texto)
        {
            //-------------------------------------sets inicio-----------------------------------------------------------
            string pat = @"sets";
            int inicio = 0;
            // Instantiate the regular expression object.
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            Match m = r.Match(texto);
            if (m.Success)
            {
                inicio = m.Index;
            }
            //----------------------------------------toknes final --------------------------------------------------------
            pat = @"tokens";
            int final = 0;
            int final2 = 0;
            // Instantiate the regular expression object.
            r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            m = r.Match(texto);
            if (m.Success)
            {
                final = m.Index;
                final2 = final - inicio;
            }
            //------------------------------------------------------------------------------------------------------------
            string primerstr = texto.Substring(0, inicio);
            string segundostr = texto.Substring(inicio, final2); //string que me interesa contiene la palabra sets
            string finalstr = texto.Substring(final); //contiene la palabra tokens

            //pre arreglo  
            string pattern2 = @"(\'\.)(?!\.)";
            string replacement2 = "\' .";
            Regex rgx2 = new Regex(pattern2);
            string result = rgx2.Replace(segundostr, replacement2);
            Console.WriteLine(result);

            string pattern = @"(\)\.)(?!\.)";
            string replacement = ") .";
            Regex rgx = new Regex(pattern);
            result = rgx.Replace(result, replacement);
            Console.WriteLine(result);

            pattern = @"[^(""|\'|<|>)]=|=[^(""|\')]";
            replacement = "(";
            Regex rgx3 = new Regex(pattern);
            result = rgx3.Replace(result, replacement);
            Console.WriteLine(result);

            pattern = @"([^\'|\.|\)])\."; //((\'|\))\.)(?!(\.))
            replacement = ")";
            rgx = new Regex(pattern);
            result = rgx.Replace(result, replacement);
            Console.WriteLine(result);

            segundostr = result;
            Console.WriteLine(segundostr);
            this.texto = primerstr + segundostr + finalstr;
        }

        private void End()
        {
            string pat = @"end\.";
            // Instantiate the regular expression object.
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            // Match the regular expression pattern against a text string.
            Match m = r.Match(texto);
            if (m.Success)
            {
                //pre arreglo  
                string pattern2 = @"end\.";
                string replacement2 = "";
                Regex rgx2 = new Regex(pattern2, RegexOptions.IgnoreCase);
                string result = rgx2.Replace(this.texto, replacement2);
                Console.WriteLine(result);
                this.texto = result;
            }
            else
            {
                mensaje = this.txtMensaje.Text = "Falta End. al final del archivo";
                MessageBox.Show("Falta End. al final del archivo", "Error", MessageBoxButtons.OK , MessageBoxIcon.Error);
                return;
            }
        }

        private void fix()
        {
            conjuntos(texto);
            convertirtokens();
            convertirsets();
            End();
        }
    }
}
