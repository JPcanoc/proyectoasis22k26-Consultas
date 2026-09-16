using System;
using System.Collections.Generic;
using System.Data;
using CapaModelo_Consultas;

namespace CapaControlador_Consultas
{
    // Inicio de código de "José Pablo Cano Cóbar" - carné: "0901-23-1727" - Fecha: "15/09/26"
   
    public class ClsControladorFiltroSimple
    {
        private readonly ClsSentenciasFiltroSimple _Sentencias =
            new ClsSentenciasFiltroSimple();

        public List<string> ConsultasFuncObtenerCampos(string NombreTabla)
        {
            if (string.IsNullOrWhiteSpace(NombreTabla))
            {
                return new List<string>();
            }

            return _Sentencias.ConsultasFuncObtenerCampos(NombreTabla);
        }


        // Operadores que se le ofrecen al usuario en el combo, en el texto que él lee.
        public List<string> ConsultasFuncObtenerOperadores()
        {
            return new List<string>
            {
                "=",
                "<>",
                ">",
                "<",
                ">=",
                "<=",
                "Contiene",
                "Comienza con",
                "Termina con"
            };
        }

        // Ejecuta la búsqueda filtrada y devuelve la página pedida.

        public DataTable ConsultasFuncBuscar(
            string NombreTabla,
            string Campo,
            string OperadorVisible,
            string Valor,
            int Pagina,
            int RegistrosPorPagina)
        {
            string OperadorSql = ConsultasFuncTraducirOperador(OperadorVisible);
            string ValorSql = ConsultasFuncPrepararValor(OperadorVisible, Valor);

            return _Sentencias.ConsultasFuncFiltrarTabla(
                NombreTabla,
                Campo,
                OperadorSql,
                ValorSql,
                Pagina,
                RegistrosPorPagina);
        }


        // Total de registros que cumplen el filtro, para la paginación.
        public int ConsultasFuncContar(
            string NombreTabla,
            string Campo,
            string OperadorVisible,
            string Valor)
        {
            string OperadorSql = ConsultasFuncTraducirOperador(OperadorVisible);
            string ValorSql = ConsultasFuncPrepararValor(OperadorVisible, Valor);

            return _Sentencias.ConsultasFuncContarFiltrados(
                NombreTabla,
                Campo,
                OperadorSql,
                ValorSql);
        }


        // Valida lo que el usuario llenó antes de ir a la base de datos.
        // Devuelve null si todo está bien, o el mensaje a mostrar si falta algo.

        public string ConsultasFuncValidarFiltro(
            string Campo,
            string OperadorVisible,
            string Valor)
        {
            if (string.IsNullOrWhiteSpace(Campo))
            {
                return "Seleccione el campo por el que desea filtrar.";
            }

            if (string.IsNullOrWhiteSpace(OperadorVisible))
            {
                return "Seleccione el operador de comparación.";
            }

            if (string.IsNullOrWhiteSpace(Valor))
            {
                return "Escriba el valor que desea buscar.";
            }

            return null;
        }

        private string ConsultasFuncTraducirOperador(string OperadorVisible)
        {
            if (string.IsNullOrWhiteSpace(OperadorVisible))
            {
                return string.Empty;
            }

            switch (OperadorVisible)
            {
                case "Contiene":
                case "Comienza con":
                case "Termina con":
                    return "LIKE";

                default:
                    return OperadorVisible;
            }
        }

        private string ConsultasFuncPrepararValor(
            string OperadorVisible,
            string Valor)
        {
            string ValorLimpio = (Valor ?? string.Empty).Trim();

            // Se escapan los comodines que el usuario haya escrito, para que
            // un "%" tecleado se busque como texto y no como comodín.
            string ValorEscapado = ValorLimpio
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            switch (OperadorVisible)
            {
                case "Contiene":
                    return "%" + ValorEscapado + "%";

                case "Comienza con":
                    return ValorEscapado + "%";

                case "Termina con":
                    return "%" + ValorEscapado;

                default:
                    return ValorLimpio;
            }
        }
    }
    // Fin del código de "José Pablo Cano Cóbar" - Carné: "0901-23-1727" - Fecha: "15/09/26"
}
