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
            var citasDia = citasProgramadas.FindAll(c => c.Day.Equals(diaSemana, StringComparison.OrdinalIgnoreCase));

            int minutosDisponibles = (HorarioFin - HorarioInicio) * 60;

            foreach (var cita in citasDia)
            {
                int inicioEnMinutos = (int)TimeSpan.Parse(cita.Hour).TotalMinutes;
                int finEnMinutos = inicioEnMinutos + int.Parse(cita.Duration);

                minutosDisponibles -= Math.Max(0, Math.Min(finEnMinutos, HorarioFin * 60) - Math.Max(inicioEnMinutos, HorarioInicio * 60));
            }

            int espaciosTotales = 0;
            int duracionCita = DuracionMinima;
            int minutosOcupados = 0;

            for (int i = 0; i < minutosDisponibles; i++)
            {
                if (minutosOcupados < duracionCita)
                {
                    if (i + duracionCita <= minutosDisponibles)
                    {
                        minutosOcupados++;
                    }
                    else
                    {
                        minutosOcupados = 0;
                    }
                }
                else
                {
                    espaciosTotales++;
                    minutosOcupados = 0;
                }
            }

            return espaciosTotales;
        }




        static async Task Main(string[] args)
        {
            string urlArchivo = "https://luegopago.blob.core.windows.net/luegopago-uploads/Pruebas%20LuegoPago/data.json";
            string diaSemanaConsulta = "lunes"; // Puedes cambiar el día de la semana que deseas consultar

            List<Cita> citasProgramadas = await CargarCitasDesdeArchivo(urlArchivo);

            int espaciosDisponibles = CalcularEspaciosDisponibles(diaSemanaConsulta, citasProgramadas);

            Console.WriteLine($"Espacios disponibles para el {diaSemanaConsulta}: {espaciosDisponibles}");
        }
    }

}
