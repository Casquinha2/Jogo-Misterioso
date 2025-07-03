    using UnityEngine;
    using UnityEngine.Video;

    public class VideoPrepare : MonoBehaviour, IInteractable
    {
        [Header("Áudio")]
        [Tooltip("Arraste aqui o AudioSource da música de fundo")]
        public AudioSource backgroundMusic;

        [Header("Vídeo")]
        [Tooltip("Arraste aqui o VideoPlayer que faz o render via câmera")]
        public VideoPlayer videoPlayer;

        void Awake()
        {
            if (videoPlayer != null)
            {
                videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                videoPlayer.targetCamera = Camera.main;
            }
            else
            {
                Debug.LogWarning("[VideoPrepare] videoPlayer não atribuído!");
            }
        }

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }


    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[VideoPrepare] Vídeo terminou.");

        // Para o vídeo (se necessário)
        vp.Stop();

        // Limpa a imagem (desliga a câmera)
        vp.targetCamera = null;

        // Alternativamente: desativa o GameObject com o vídeo, ou coloca uma tela preta
    }


    // IInteractable
    // Só retorna true se ainda não estiver a tocar
    public bool CanInteract()
    {
        if (videoPlayer == null)
            return false;
        return !videoPlayer.isPlaying;
    }

    // Chamado pelo InteractionDetector quando o jogador pressiona o botão
    public void Interact()
    {
        Debug.Log("[VideoPrepare] Interact() chamado.");

        // Pausa a música de fundo
        if (backgroundMusic != null && backgroundMusic.isPlaying)
        {
            backgroundMusic.Pause();
            Debug.Log("[VideoPrepare] Música de fundo pausada.");
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("[VideoPrepare] videoPlayer não atribuído!");
            return;
        }

        videoPlayer.targetCamera = Camera.main;

        // Se não estiver preparado, prepara e então toca
        if (!videoPlayer.isPrepared)
        {
            Debug.Log("[VideoPrepare] Preparando o vídeo...");
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += vp =>
            {
                vp.Play();
                Debug.Log("[VideoPrepare] Vídeo preparado e agora tocando.");
            };
        }
        else
        {
            Debug.Log("[VideoPrepare] Vídeo já preparado — tocando imediatamente.");
            videoPlayer.Play();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[VideoPrepare] Player saiu do trigger via OnTriggerExit2D.");

            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
                Debug.Log("[VideoPrepare] Vídeo parado.");
            }

            if (backgroundMusic != null && !backgroundMusic.isPlaying)
            {
                backgroundMusic.UnPause();
                Debug.Log("[VideoPrepare] Música de fundo retomada.");
            }
        }
    }


}