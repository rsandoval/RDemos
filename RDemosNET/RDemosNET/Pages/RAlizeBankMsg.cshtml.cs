using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using RDemosNET.Models;

namespace RDemosNET
{
    public class RAlizeBankMsgModel : PageModel
    {
        [BindProperty]
        public string MessageContents { get; set; }

        public string MessageDescription { get; set; }

        public string MessageShortDescription { get; set; }


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
        public void OnPostProcessComment(string txtContents)
        {
            string strComment = txtContents;
            MessageContents = strComment;
            BankMessageCharacterizer characterizer = new BankMessageCharacterizer(strComment);
            MessageDescription = characterizer.GetDescription();
            MessageShortDescription = characterizer.GetSimpleDescription();
        }

        public void OnPostRandomSample()
        {
            Random randomNum = new Random(DateTime.Now.Millisecond);
            string[] samples = {
                "Hola Adrián, no puedo hacer transferencias, se queda pegado en la llamada de la clave dinámica.",
                "Estimada María Soledad. Ante todo desearte un estupendo año¡¡¡¡...  Al revisar la cuenta corriente figura un cargo por seis mil y algo pesos. De acuerdo a lo contratado no habría recargo y yo no he utilizado aún ninguno de mis productos. Me puedes contar a que se debe?...  Agradecería me indicaras también, si sabes algo del re financiamiento de mi propiedad. Estoy a la espera de esa respuesta para cerrar mis otros productos y transferir todo a esta cuenta corriente.  Agradezco tus gestiones,  Sa",
                "Estimados señores: Quisiera comentarles que en tres ocasiones dentro de este año he intentado contactar a mi ejecutiva por esta vía, no habiendo tenido respuesta de parte ",
                "El 1 de marzo compre el SOAP con mi tarjeta de credito mastercard y a la fecha aun no recibo los creditos cabify ofrecidos por la compra.",
                "estimada Gabriela podrias trnsferir de mi cuenta corriente 100.000 pesos a mi deposito a plazo atenta a su respuesta",
                "Hola  quisiera dar de baja un seguro de fraude que tengo contratado.   desde ya gracias,  saludos. ",
                "Katia,  Me podrias por favor indicar cuales son als condiciones de prepago del credito de consumo con tasa preferencial.  Quiero solicitarlo pero necesito saber cuales son las condiciones de prepago.  Muchas gracias.  Carolina",
                "Gina  Necesito que me permitan usar tarjeta de crédito en Asia, desde el día 12 de enero al día 5 de febrero 2018; además, necesito que me informen monto de comisión por sacar plata y por comprar con la tarjeta.  Gracias,  Pamela ",
                "Hola, requiero las cartolas históricas de mi cuenta corriente desde enero del 2013 a Marzo del 2016. No me aparecen en el portal las antiguas. Solo desde abril 2016.  Muchas ",
                "Estimada Marcela Agradeceré me ayude con este cargo ya que está duplicado, solo tiene que haber 1: 24-Ago-2018  PORTEZUELO VITACURA SANTIAGO CC 01-03  ",
                "hola Carolina, por favor pagar credito de consumo.  muy felices fiestas.  mil gracias",
                "Hola Claudia. Por favor te pido me puedan contactar para revisar que opciones me pueden ofrecer para un crédito de consumo. Muchas Gracias.",
                "Nicolas porfavor da termino al cobro de los Seguros de Hogar que tengo solo deseo continuar con los seguros de fraude agradecida saluda atte  Patricia",
                "Al revisar el día de hoy mi cuenta me percato que me han cobrado de forma automática el pago mínimo de la linea de crédito cargándomelo en la corriente, siendo que este fue pagado el 28 de febrero. Poseo el comprobante de pago realizado el 28 de febrero, y también el comprobante del cobro que me han realizado el día de hoy. "
            };
            string strComment = samples[randomNum.Next(samples.Length)];
            MessageContents = strComment;
            BankMessageCharacterizer characterizer = new BankMessageCharacterizer(strComment);
            MessageDescription = characterizer.GetDescription();
            MessageShortDescription = characterizer.GetSimpleDescription();
        }

        public string GetDescriptionAsHTML(BankMessageCharacterizer characterizer)
        {
            string descAsHTML = characterizer.GetDescription();


            return descAsHTML;
        }

    }
}