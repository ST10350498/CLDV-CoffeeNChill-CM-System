using Microsoft.Azure.Functions.Worker;
using System.IO;
using System.Net;
using System.Threading.Tasks;

{
    {
        {
        }

        {
            {


                {
                    }
                    catch (FileNotFoundException) { }
                }

                {
                }

            }
            catch (Exception ex)
            {
            }
        }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
