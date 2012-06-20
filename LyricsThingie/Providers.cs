using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace LyricsThingie {
    class Providers {
        private static Encoding ISOEncoding = Encoding.GetEncoding("ISO-8859-1");

        public static string GetFromLyricWiki(string artist, string title) {
            string rep = string.Empty;

            artist = HttpUtility.UrlEncode((artist + ""), ISOEncoding).Replace(".", "%2E");
            title = HttpUtility.UrlEncode((title + ""), ISOEncoding).Replace(".", "%2E");

            //Utilizar o serviço PUSH para verificar se obtemos a música.
            WebRequest request = HttpWebRequest.Create(string.Format("http://lyrics.wikia.com/api.php?func=getSong&artist={0}&song={1}&fmt=xml", artist, title));
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());

            XmlDocument doc1 = new XmlDocument();
            string req = reader.ReadToEnd();
            doc1.LoadXml(req);

            //Verificar se eles tem a letra
            XmlNode firstNode = doc1.SelectSingleNode("//lyrics");
            if (firstNode == null) return string.Empty;
            if (doc1.SelectSingleNode("//lyrics").InnerText == "Not found") return string.Empty;

            //Caso tenham, fazer uma requisição a URL onde está a letra completa
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(doc1.SelectSingleNode("//url").InnerText);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@class='lyricbox']");

            if (node == null) return string.Empty;

            //Remover elementos desnecessários
            node.FirstChild.Remove();
            node.LastChild.Remove();
            node.LastChild.Remove();
            node.LastChild.Remove();
            node.LastChild.Remove();

            //Trocar os <br> por quebras de linha convencionais
            rep = Regex.Replace(WebUtility.HtmlDecode(node.InnerHtml), "\\s*<br ?\\/?>\\s*", "\r\n");
            //Remover qualquer tag html presente
            rep = Regex.Replace(rep, "<[^>]+>", "");

            return rep;
        }

        public static string GetFromTerra(string artist, string title) {
            string rep = string.Empty;

            artist = (artist + "").ToLowerInvariant();
            title = (title + "").ToLowerInvariant();

            //Obter a letra da música
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(string.Format("http://letras.mus.br/winamp.php?t={0}-{1}", HttpUtility.UrlEncode(artist, ISOEncoding), HttpUtility.UrlEncode(title, ISOEncoding)));
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='letra']/p");

            //Se encontrar a letra, retorna
            if (node != null) {
                rep = WebUtility.HtmlDecode(node.InnerText);
                return rep;
            }

            //Se não encontrar, vamos trocar os "&" por "e" e ver se encontramos
            if (artist.Contains("&") || title.Contains("&")) {
                artist = artist.Replace('&', 'e');
                title = title.Replace('&', 'e');

                doc = web.Load(string.Format("http://letras.mus.br/winamp.php?t={0}-{1}", HttpUtility.UrlEncode(artist, ISOEncoding), HttpUtility.UrlEncode(artist, ISOEncoding)));
                node = doc.DocumentNode.SelectSingleNode("//div[@id='letra']/p");

                if (node != null) {
                    rep = WebUtility.HtmlDecode(node.InnerText);
                }
            }

            return rep;
        }

    }
}
