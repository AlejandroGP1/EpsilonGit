public interface IAppControlable
{
    string NombreApp { get; } // Debe coincidir con el nombre en TocandoIcono
    bool SoportaGesto(string gesto); 
    void EjecutarAccion(string gesto);
    void AlEntrar();
    void AlSalir();
}