using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;


namespace ProgramacionCitas
{
    public class Cita
    {
        public string Day { get; set; }
        public string Hour { get; set; }
        public string Duration { get; set; }
    }



    public class ProgramaCitas
    {
        private const int HorarioInicio = 9;
        private const int HorarioFin = 17;
        private const int DuracionMinima = 30;
        private const int DuracionMaxima = 90;



        private static async Task<List<Cita>> CargarCitasDesdeArchivo(string url)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string contenido = await httpClient.GetStringAsync(url);
                    return JsonSerializer.Deserialize<List<Cita>>(contenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el archivo: {ex.Message}");
                return new List<Cita>();
            }
        }


        private static int CalcularEspaciosDisponibles(string diaSemana, List<Cita> citasProgramadas)
        {
            var citasDia = citasProgramadas
                .Where(c => c.Day.Equals(diaSemana, StringComparison.OrdinalIgnoreCase))
                .ToList();

            int minutosDisponibles = (HorarioFin - HorarioInicio) * 60;

            foreach (var cita in citasDia)
            {
                int inicioEnMinutos = (int)TimeSpan.Parse(cita.Hour).TotalMinutes;
                int finEnMinutos = inicioEnMinutos + int.Parse(cita.Duration);

                minutosDisponibles -= Math.Max(0, Math.Min(finEnMinutos, HorarioFin * 60) - Math.Max(inicioEnMinutos, HorarioInicio * 60));
            }

            const int duracionCita = 30; 
            int espaciosTotales = 0;
            int intervaloInicio = HorarioInicio * 60; 

            for (int i = intervaloInicio; i <= (HorarioFin * 60) - duracionCita; i += duracionCita)
            //                 540(9)   ;   <=        990(4:30)                ;            30
            {
                if (!citasDia.Any(cita => IsCitaEnIntervalo(cita, i, i + duracionCita)))
                {
                    espaciosTotales++;
                }
            }

            return espaciosTotales;
        }


        // Método auxiliar para verificar si hay una cita programada en el intervalo de tiempo dado
        private static bool IsCitaEnIntervalo(Cita cita, int inicio, int fin)
        {
            int inicioCita = (int)TimeSpan.Parse(cita.Hour).TotalMinutes;
            int finCita = inicioCita + int.Parse(cita.Duration);

            return inicioCita < fin && finCita > inicio;
            //          555   < 570 &&  615    >  540
            //         9:15   < 9:30 && 10:15  >  9:00
        }




        static async Task Main(string[] args)
        {
            string urlArchivo = "https://luegopago.blob.core.windows.net/luegopago-uploads/Pruebas%20LuegoPago/data.json";
            string[] diasSemana = { "lunes", "martes", "miércoles", "jueves", "viernes", "sábado", "domingo" };
            //string diaSemanaConsulta = "viernes"; 

            List<Cita> citasProgramadas = await CargarCitasDesdeArchivo(urlArchivo);

            // Iterar sobre todos los días de la semana
            foreach (var diaSemana in diasSemana)
            {
                // Calcular espacios disponibles para el día actual
                int espaciosDisponibles = CalcularEspaciosDisponibles(diaSemana, citasProgramadas);

                // Mostrar la disponibilidad para el día actual
                Console.WriteLine($"Espacios disponibles para el {diaSemana}: {espaciosDisponibles}");
            }

            //int espaciosDisponibles = CalcularEspaciosDisponibles(diaSemanaConsulta, citasProgramadas);

            //Console.WriteLine($"Espacios disponibles para el {diaSemanaConsulta}: {espaciosDisponibles}");
        }
    }

}
