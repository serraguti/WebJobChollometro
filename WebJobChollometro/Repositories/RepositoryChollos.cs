using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebJobChollometro.Data;
using WebJobChollometro.Models;

namespace WebJobChollometro.Repositories
{
    public class RepositoryChollos
    {
        private ChollometroContext context;

        public RepositoryChollos(ChollometroContext context)
        {
            this.context = context;
        }

        //VAMOS A REALIZAR EL ID AUTOMATICO
        //RECUPERAMOS EL MAXIMO DE LA TABLA CHOLLOS Y LE SUMAMOS UNO
        //Y LO HAREMOS CON LAMBDA
        private int GetMaxIdChollo()
        {
            if (this.context.Chollos.Count() == 0)
            {
                return 1;
            }
            else
            {
                //RECUPERAMOS EL MAXIMO ID CON LAMBDA
                int maximo = this.context.Chollos.Max( x => x.IdChollo ) + 1;
                return maximo;
            }
        }

        //VAMOS A TENER UN METODO PARA LEER LOS DATOS DE CHOLLOMETRO
        //Y CONVERTIRLOS EN UNA List<Chollo>
        private List<Chollo> GetChollosWeb()
        {
            string url = "https://www.chollometro.com/rss";
            //LOS TENEMOS QUE RECUPERAR DE OTRA FORMA PORQUE TIENE
            //RESTRICCIONES, DEBEMOS HACER PENSAR A LA PAGINA
            //QUE ACCEDEMOS DESDE UN EXPLORADOR WEB
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = @"text/html application/xhtml+xml, *,*";
            request.Referer = @"https://www.chollometro.com/";
            request.Headers.Add("Accept-Language", "es-ES");
            request.UserAgent = @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
            request.Host = @"www.chollometro.com";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //LO QUE NOS DA ES UN Stream, DEBEMOS CONVERTIR DICHO FLUJO
            //EN UN String DE XML.
            string xmlData = "";

            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                xmlData = stream.ReadToEnd();
            }
            //MEDIANTE LINQ TO XML RECORREMOS LOS DATOS XML RECIBIDOS
            XDocument document = XDocument.Parse(xmlData);
            var consulta = from datos in document.Descendants("item")
                           select datos;
            List<Chollo> chollos = new List<Chollo>();
            int idchollo = this.GetMaxIdChollo();
            //RECORREMOS TODOS LOS CHOLLOS
            foreach (var item in consulta)
            {
                Chollo chollo = new Chollo();
                chollo.IdChollo = idchollo;
                chollo.Titulo = item.Element("title").Value;
                chollo.Descripcion = item.Element("description").Value;
                chollo.Link = item.Element("link").Value;
                chollo.Fecha = DateTime.Now;
                //INCREMENTAMOS EL ID DE CADA CHOLLO
                idchollo += 1;
                chollos.Add(chollo);
            }
            return chollos;
        }

        //METODO PARA INSERTAR CHOLLOS EN NUESTRA BBDDD
        public void PopulateChollos()
        {
            //RECUPERAMOS LOS CHOLLOS DE LA WEB
            List<Chollo> chollos = this.GetChollosWeb();
            //RECORREMOS TODOS LOS CHOLLOS Y LOS INSERTAMOS EN CONTEXT
            foreach (Chollo chollo in chollos)
            {
                this.context.Chollos.Add(chollo);
            }
            this.context.SaveChanges();
        }
    }
}
