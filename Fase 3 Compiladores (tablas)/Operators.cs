using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fase_3_Compiladores__tablas_
{
    class Operators
    {
        public Operators next, back;
        public List<string> operadores;
        public string lado;
        public Operators(string lado)
        {
            this.next = this.back = null;
            operadores = new List<string>();
            lado = "";
        }
    }
    class listOperator
    {
        Operators head, tail;
        public listOperator()
        {
            head = tail = null;
        }
        bool isempty()
        {
            return this.head == null;
        }
        public void push(List<string> l, string lado)
        {
            Operators it = new Operators(lado);
            it.operadores = l;
            if (this.isempty())
            {
                this.head = it;
                this.tail = this.head;
                return;
            }
            this.tail.next = it;
            this.tail = it;
            return;
        }

        public bool containselemento(string s)
        {
            if (this.isempty() == true)
            {
                return false;
            }
            else
            {
                Operators i = this.head;
                bool b = false;
                while (i != null && b == false)
                {
                    if (i.operadores.Contains(s) == true)
                    {
                        b = true;
                    }
                    else
                        i = i.next;
                }
                return b;
            }
        }
    }
}
