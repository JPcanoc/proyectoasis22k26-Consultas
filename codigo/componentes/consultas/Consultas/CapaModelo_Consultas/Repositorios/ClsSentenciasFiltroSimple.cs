using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Text.RegularExpressions;

namespace CapaModelo_Consultas
{
    // Inicio de código de "José Pablo Cano Cóbar" - carné: "0901-23-1727" - Fecha: "15/09/26"
    
    public class ClsSentenciasFiltroSimple
    {
        private readonly ClsConexion _Conexion = new ClsConexion();

        private static readonly string[] _OperadoresPermitidos =
        {
            "=", "<>", ">", "<", ">=", "<=", "LIKE"
        };

        public List<string> ConsultasFuncObtenerCampos(string NombreTabla)
        {
            ConsultasMetValidarIdentificador(NombreTabla, "tabla");

            List<string> Campos = new List<string>();

            string Consulta =
                "SELECT COLUMN_NAME " +
                "FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = DATABASE() " +
                "AND TABLE_NAME = ? " +
                "ORDER BY ORDINAL_POSITION;";

            using (OdbcConnection Conexion = _Conexion.ConsultasFuncConexion())
            {
                using (OdbcCommand Cmd = new OdbcCommand(Consulta, Conexion))
                {
                    Cmd.Parameters.AddWithValue("?", NombreTabla);

                    using (OdbcDataReader Lector = Cmd.ExecuteReader())
                    {
                        while (Lector.Read())
                        {
                            Campos.Add(Lector["COLUMN_NAME"].ToString());
                        }
                    }
                }
            }

            return Campos;
        }


        // Trae una página de registros de la tabla aplicando un filtro opcional.
        // Si Campo u Operador vienen vacíos, devuelve la tabla sin filtrar.
        public DataTable ConsultasFuncFiltrarTabla(
            string NombreTabla,
            string Campo,
            string Operador,
            string Valor,
            int Pagina,
            int RegistrosPorPagina)
        {
            ConsultasMetValidarIdentificador(NombreTabla, "tabla");

            bool HayFiltro = ConsultasFuncHayFiltro(Campo, Operador);

            if (HayFiltro)
            {
                ConsultasMetValidarCampo(NombreTabla, Campo);
                ConsultasMetValidarOperador(Operador);
            }

            DataTable DtResultado = new DataTable();

            int Inicio = (Pagina - 1) * RegistrosPorPagina;

            string Consulta = "SELECT * FROM " + NombreTabla;

            if (HayFiltro)
            {
                Consulta += " WHERE " + Campo + " " + Operador + " ?";
            }

            Consulta += " LIMIT ? OFFSET ?;";

            using (OdbcConnection Conexion = _Conexion.ConsultasFuncConexion())
            {
                using (OdbcCommand Cmd = new OdbcCommand(Consulta, Conexion))
                {
                    // ODBC usa parámetros posicionales: el orden en que se agregan
                    // debe ser el mismo orden en que aparecen los "?" en la sentencia.
                    if (HayFiltro)
                    {
                        Cmd.Parameters.AddWithValue("?", Valor ?? string.Empty);
                    }

                    Cmd.Parameters.AddWithValue("?", RegistrosPorPagina);
                    Cmd.Parameters.AddWithValue("?", Inicio);

                    using (OdbcDataAdapter DaResultado = new OdbcDataAdapter(Cmd))
                    {
                        DaResultado.Fill(DtResultado);
                    }
                }
            }

            return DtResultado;
        }


        // Cuenta cuántos registros cumplen el filtro. Lo necesita la paginación
        // para saber cuántas páginas dibujar.
        public int ConsultasFuncContarFiltrados(
            string NombreTabla,
            string Campo,
            string Operador,
            string Valor)
        {
            ConsultasMetValidarIdentificador(NombreTabla, "tabla");

            bool HayFiltro = ConsultasFuncHayFiltro(Campo, Operador);

            if (HayFiltro)
            {
                ConsultasMetValidarCampo(NombreTabla, Campo);
                ConsultasMetValidarOperador(Operador);
            }

            string Consulta = "SELECT COUNT(*) FROM " + NombreTabla;

            if (HayFiltro)
            {
                Consulta += " WHERE " + Campo + " " + Operador + " ?";
            }

            Consulta += ";";

            using (OdbcConnection Conexion = _Conexion.ConsultasFuncConexion())
            {
                using (OdbcCommand Cmd = new OdbcCommand(Consulta, Conexion))
                {
                    if (HayFiltro)
                    {
                        Cmd.Parameters.AddWithValue("?", Valor ?? string.Empty);
                    }

                    object Total = Cmd.ExecuteScalar();

                    return Total == null || Total == DBNull.Value
                        ? 0
                        : Convert.ToInt32(Total);
                }
            }
        }

        private bool ConsultasFuncHayFiltro(string Campo, string Operador)
        {
            return !string.IsNullOrWhiteSpace(Campo)
                && !string.IsNullOrWhiteSpace(Operador);
        }

        private void ConsultasMetValidarIdentificador(
            string Identificador,
            string Descripcion)
        {
            if (string.IsNullOrWhiteSpace(Identificador))
            {
                throw new ArgumentException(
                    "El nombre de la " + Descripcion + " no puede estar vacío.");
            }

            if (!Regex.IsMatch(Identificador, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new ArgumentException(
                    "El nombre de la " + Descripcion +
                    " contiene caracteres no válidos: " + Identificador);
            }
        }

        private void ConsultasMetValidarCampo(string NombreTabla, string Campo)
        {
            ConsultasMetValidarIdentificador(Campo, "columna");

            List<string> CamposReales = ConsultasFuncObtenerCampos(NombreTabla);

            bool Existe = CamposReales.Exists(
                CampoReal => string.Equals(
                    CampoReal,
                    Campo,
                    StringComparison.OrdinalIgnoreCase));

            if (!Existe)
            {
                throw new ArgumentException(
                    "La columna '" + Campo + "' no existe en la tabla '" +
                    NombreTabla + "'.");
            }
        }

        private void ConsultasMetValidarOperador(string Operador)
        {
            if (Array.IndexOf(_OperadoresPermitidos, Operador) < 0)
            {
                throw new ArgumentException(
                    "El operador '" + Operador + "' no está permitido.");
            }
        }
    }
    // Fin del código de "José Pablo Cano Cóbar" - Carné: "0901-23-1727" - Fecha: "15/09/26"
}
