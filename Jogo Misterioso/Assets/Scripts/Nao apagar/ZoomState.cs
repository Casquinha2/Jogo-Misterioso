public static class ZoomState
{
  // armazena se já estamos em zoom ou não
  public static bool IsZoomed { get; set; } = false;
  
  // tamanho original da câmera (=-1 indica "não inicializado")
  public static float OriginalSize { get; set; } = -1f;
}
