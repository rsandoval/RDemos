using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RDemosNET.Models;


namespace RDemosNET
{
    public class RAlizeMsgModel : PageModel
    {
        [BindProperty]
        public string MessageContents { get; set; }

        public string MessageDescription { get; set; }


        public void OnGet()
        {

        }

        public void OnPostProcessDocument(string txtContents)
        {

            MessageCharacterizer characterizer = new MessageCharacterizer(txtContents);
            MessageContents = txtContents;
            MessageDescription = characterizer.GetDescription();
        }
        public void OnPostRandomSample()
        {
            Random randomNum = new Random(DateTime.Now.Millisecond);
            string[] samples = {
                "Por favor modificar plan multimedia de las siguiente líneas:  978978790 - 999909988 - 999999779 - 989989990 - 979909999 - 999997898  El plan que tienen que tener es el de minutos ilimitados + 99 GB de datos.  Saludos.",
                "Estimados buenas tardes. Junto con Saludar, agradecería gestionar Activación de Plan de Datos 9 Gb (Plan w89) a Línea N° 979997999. Muchas Gracias.",
                "por favor revisar las lineas 99999909999 y 99999989909, tienen problemas con la recepcion de mensajes. se llamó a servicio tecnico y lo que indican es lo siguiente No tiene recepción de mensajería entrante solo voz" };
            string strComment = samples[randomNum.Next(samples.Length)];
            MessageContents = strComment;
            MessageCharacterizer characterizer = new MessageCharacterizer(strComment);
            MessageDescription = characterizer.GetDescription();
        }

    }
}