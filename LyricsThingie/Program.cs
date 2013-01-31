using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagLib.Id3v2;
using System.Net;
using System.Xml;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;

namespace LyricsThingie
{
    class Program
    {
        static void Main(string[] args)
        {
            //string directory = args[0];
            string directory = "D:\\Musicas Variadas";
            StreamWriter write = null;

            if (args.Length >= 2)
            {
                string errors = args[1];
                write = new StreamWriter(errors);
                write.WriteLine("Arquivos cuja letra nao foi encontrada:");
            }

            bool forceDownload = Convert.ToBoolean(ConfigurationManager.AppSettings["ForceDownload"]);

            Console.WriteLine("Iniciando processo, músicas que já possuem letra serão puladas.");

            Parallel.ForEach(Directory.GetFiles(directory, "*.mp3", SearchOption.AllDirectories), fileName =>
            {
                TagLib.File file = TagLib.File.Create(fileName);

                if (!string.IsNullOrEmpty(file.Tag.Lyrics) && file.Tag.Lyrics.Contains("Unfortunately, we are not licensed to display"))
                {
                    file.Tag.Lyrics = "";
                }

                if (!string.IsNullOrEmpty(file.Tag.Lyrics) && !forceDownload) { return; }

                try
                {
                    {
                        file.Tag.Lyrics = GetLyricsForSong(file.Tag.FirstAlbumArtist, file.Tag.Title);
                        file.Save();
                        Console.Write("Procurando letra para {0} - {1}... OK\r\n", file.Tag.FirstAlbumArtist, file.Tag.Title);
                    }
                }
                catch (KeyNotFoundException)
                {
                    if (write != null)
                    {
                        write.WriteLine("{0} | {1} - {2}", fileName, file.Tag.FirstAlbumArtist, file.Tag.Title);
                        write.Flush();
                    }
                    Console.Write("Procurando letra para {0} - {1}... Não foi encontrada letra.\r\n", file.Tag.FirstAlbumArtist, file.Tag.Title);
                }
                catch (Exception)
                {
                    if (write != null)
                    {
                        write.WriteLine("{0} | {1} - {2}", fileName, file.Tag.FirstAlbumArtist, file.Tag.Title);
                        write.Flush();
                    }
                    Console.Write("Procurando letra para {0} - {1}... Houve um problema ao obter a letra, tente novamente.\r\n", file.Tag.FirstAlbumArtist, file.Tag.Title);

                }
            });

            if (write != null)
            {
                write.Dispose();
            }

            Console.WriteLine("Processo finalizado, aperte enter para sair.");
            Console.ReadLine();
        }

        static string GetLyricsForSong(string artist, string title)
        {
            string letra = string.Empty;

            letra = Providers.GetFromLyricWiki(artist, title);

            if (string.IsNullOrEmpty(letra))
            {
                letra = Providers.GetFromTerra(artist, title);
            }

            if (string.IsNullOrEmpty(letra)) throw new KeyNotFoundException();

            return letra;
        }
    }
}
