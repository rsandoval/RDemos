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
            if (String.IsNullOrEmpty(txtContents))
            {
                MessageDescription = "No hay mensaje a procesar.";
                return;
            }
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
                "por favor revisar las lineas 99999909999 y 99999989909, tienen problemas con la recepcion de mensajes. se llamó a servicio tecnico y lo que indican es lo siguiente No tiene recepción de mensajería entrante solo voz",
                "Se solicita la habilitación de 7 lineas con datos móviles con plan de empresa informado en el documento.  Muchas gracias",
                "Estimada.   Favor confirmar que plan tiene la linea 999978809",
                "Descripción escrita por usuario: 99.899.099-9 Activación de ID privado línea 998999999",
                "favor eliminar suscripcion de JUEGOS GAMELOFT línea 999999789",
                "Fvr deshabilitar plan america linea 989097999  contacto Francisco Salcie 999809999 ",
                "favor gestionar baja de servicios que indica, ya que cliente sigue traficando llamadas a LDI las que ya se solcitó eliminar en REQ000000999799, de dicho código de cliente deben",
                "Estimados. Solicito bloquear el envìo de SMS para las lìneas adjuntadas en el excel. Saludos Cordiales",
                "solicito alta de servicio mensajería para linea 998997899",
                "Estimados, favor su apoyo con la activación de estas dos numeraciones. gracias. teléfono 999999899, el imei es 999999080909909 teléfono 999999999, el Imei es 999999080889809  gracias!",
                "Estimados buenas tardes. Junto con Saludar, agradecería gestionar Eliminación de Plan de Datos 9 Gb (Plan w89) a Línea N°989899998. Muchas Gracias."
            };
            string strComment = samples[randomNum.Next(samples.Length)];
            MessageContents = strComment;
            MessageCharacterizer characterizer = new MessageCharacterizer(strComment);
            MessageDescription = characterizer.GetDescription();
        }

    }
}