# 📖 Dicionário Unity - BomberTeen

Bem-vindo ao dicionário de conceitos do projeto! Este documento serve como um guia rápido para entender os principais componentes, configurações e scripts utilizados no desenvolvimento do jogo, organizados de forma lógica para facilitar a consulta.

---

## 🎨 1. Gráficos e Estrutura Visual

### Sprites e Texturas
*   **Sprites:** São as imagens 2D usadas no jogo.
*   **Sprite Sheets:** Uma única imagem que contém múltiplos sprites (como os quadros de uma animação de um personagem). A Unity consegue fatiar essa imagem para usar cada quadro individualmente.
*   **Filter Mode = Point (no filter):** Configuração essencial para jogos em Pixel Art. Ela desativa o desfoque (suavização) da imagem quando ela é ampliada, mantendo os pixels nítidos e "quadradinhos" originais.

### UI (Interface de Usuário)
*   **Canvas:** A área principal (o "quadro") onde todos os elementos de interface (botões, textos, painéis) são desenhados na tela.
*   **Scale Mode = Match Width or Height:** Configuração do *Canvas Scaler* que faz a interface se adaptar automaticamente a diferentes resoluções e tamanhos de tela.
*   **Match = 1 (ou 0, 0.5):** Trabalha em conjunto com a opção acima. Define se a escala vai priorizar a adaptação pela altura (1), pela largura (0) ou um meio-termo (0.5) da tela.
*   **RectTransform:** É o substituto do componente *Transform* exclusivo para elementos de UI. Ele trabalha com Ancoramento (Anchors), permitindo "grudar" um botão no canto da tela independentemente do tamanho do monitor.
*   **CanvasGroup:** Um componente excelente para menus. Adicionado a um painel pai, permite controlar a transparência (Alpha) e bloquear os cliques (Interactable/Blocks Raycasts) de todos os botões filhos de uma única vez.

---

## 🏗️ 2. Fundamentos da Scene (Cena)

### GameObjects e Prefabs
*   **GameObject:** O bloco de construção fundamental da Unity. Tudo na cena é um GameObject (personagens, cenário, câmera, luzes). Eles são "vazios" por padrão; o que dá vida a eles são os *Componentes*.
*   **Transform:** O componente obrigatório presente em todo e qualquer GameObject. Ele guarda a Posição, Rotação e Escala do objeto no mundo tridimensional ou bidimensional.
*   **Prefabs (Prefabricados):** São "moldes" ou "clones" de GameObjects pré-configurados salvos nos arquivos do projeto. Você cria um modelo (ex: um tipo específico de inimigo ou uma bomba) e salva como Prefab. Se você alterar o Prefab, todos os inimigos daquele tipo espalhados pelo jogo são atualizados automaticamente.
*   **Layers (Camadas):** Usadas para categorizar e separar objetos. Muito úteis para definir regras de colisão: você pode fazer com que a layer "Inimigo" não colida com a layer "Inimigo", mas colida com "Player" ou "Cenário".

### Tilemap (Cenários em Grade)
*   **Camera:** O "olho" do jogador. Em jogos 2D (como o BomberTeen), o componente Camera é configurado para o modo de projeção **Orthographic** (Ortográfico), o que remove a perspectiva de profundidade 3D, achatando a imagem.
*   **Grid:** O componente "Pai" obrigatório de todo Tilemap. Ele define o layout e o tamanho exato de cada célula/quadradinho da grade do mundo.
*   **Tilemap:** Uma grade invisível acoplada ao Grid onde você pode "pintar" o cenário do jogo usando bloquinhos 2D, facilitando muito a criação de fases (level design).
*   **Tilemap Renderer:** É o componente que desenha efetivamente o Tilemap, fazendo com que ele apareça na tela.

---

## ⚙️ 3. Componentes Físicos e Colisões (Physics 2D)

*   **Sprite Renderer:** O componente responsável por desenhar a imagem do Sprite na tela. Sem ele, o GameObject fica invisível.
*   **Rigidbody 2D:** Adiciona física ao objeto. Faz com que ele seja afetado pela gravidade, tenha massa e responda a forças. (Essencial para o jogador ou inimigos se moverem usando a física da engine).
*   **PhysicsMaterial2D:** Um arquivo que define propriedades do material do objeto, como atrito (friction) e "quique" (bounciness). Útil para fazer uma bola quicar ou para fazer paredes escorregadias.
*   **Area Effector 2D:** Um componente que aplica forças contínuas dentro de uma área. Pode ser usado para criar esteiras rolantes, correntes de vento ou água puxando o jogador.

### Colisores (Colliders)
*   **Box Collider 2D:** Uma área de colisão invisível em formato retangular ao redor do objeto.
*   **Circle Collider 2D:** Uma área de colisão invisível em formato circular.
*   **Tilemap Collider 2D:** Cria colisões automaticamente baseando-se no formato de cada bloquinho (Tile) pintado no Tilemap.
*   **Composite Collider 2D:** Junta vários colisores menores e próximos (como os do Tilemap Collider 2D) fundindo-os em um único grande contorno de colisão contínuo, otimizando muito a performance do jogo e evitando que o jogador "fique preso" em emendas de bloquinhos.
*   **IsTrigger:** Uma caixinha de seleção presente nos componentes de Collider. Se estiver marcada, o objeto perde a colisão física rígida ("parede"), mas passa a funcionar como um sensor ou alarme invisível (ex: área de detecção de um inimigo, um botão no chão, ou um power-up coletável).

---

## 🎬 4. Animações e Inteligência Artificial

*   **Animation na Hierarquia:** As animações são salvas em arquivos `.anim`, e na hierarquia do GameObject é onde acoplamos o componente para tocar essas animações.
*   **Animator:** O "cérebro" das animações. É um componente que controla *quando* e *qual* animação deve tocar (ex: Correndo, Parado, Morrendo) usando uma máquina de estados visuais.
*   **Trigger (Gatilho de Animação):** Um tipo de parâmetro no Animator que funciona como um "botão de pulso". Você "aperta" (via script) e ele dispara uma transição de animação uma única vez (ex: a animação de plantar uma bomba ou de receber dano), voltando ao estado normal logo em seguida.
*   **anim.SetBool("Var", value):** Comando de código em C# usado para alterar um parâmetro do tipo Boolean (Verdadeiro ou Falso) dentro do Animator. Ex: `anim.SetBool("IsRunning", true)` faz o Animator saber que deve tocar a animação de corrida.
*   **NavMesh (Inteligência Artificial de Inimigos):** Sistema de navegação de IA da Unity. Ele "mapeia" onde é o chão caminhável da fase e permite que inimigos calculem rotas inteligentes para perseguir o jogador, desviando de obstáculos automaticamente.

---

## 💻 5. Scripts e Programação (C#)

### Conceitos Básicos
*   **Add a Script in an object:** Para um código interagir com o jogo, ele deve ser adicionado como um Componente a um GameObject ativo na cena.
*   **GetComponent<Tipo>():** Um dos comandos mais importantes. Ele faz o script procurar e referenciar ("pegar") outro componente que está acoplado no mesmo GameObject. Ex: `GetComponent<Rigidbody2D>()` permite acessar as configurações físicas do objeto via código.
*   **Script Static:** Variáveis ou métodos declarados como estáticos (`static`) pertencem à classe em si, e não a um objeto específico. Eles existem globalmente, mantendo seus valores independente das instâncias na cena. Geralmente usado para Gerenciadores Globais (Managers) ou Banco de Dados locais.

### Variáveis e Atributos de Código
*   **Vector3:** Uma estrutura matemática que armazena três números: X, Y e Z. Usada universalmente na Unity para representar Posições no espaço, Direções ou Escala.
*   **Header:** Um atributo visual de organização (ex: `[Header("Configurações do Player")]`). Ele não afeta o código, mas cria um título bonitinho e organizado no painel do Inspector da Unity para quem estiver editando o jogo.
*   **[SerializeField]:** Um atributo colocado acima de uma variável privada (ex: `[SerializeField] private float speed;`) que permite que ela apareça e seja editada diretamente no painel Inspector da Unity, sem precisar torná-la pública para outros scripts acessarem.
*   **Time.deltaTime / Time.fixedDeltaTime:** Representam a diferença de tempo que passou desde o último frame desenhado. Usados para multiplicar valores de movimento, garantindo que a velocidade do jogo seja constante independente do FPS do computador (seja 30 ou 144 FPS).

### Métodos de Ciclo de Vida (Life Cycle)
*   **Awake():** A primeira função chamada quando o script é carregado, antes mesmo do `Start()`. É o melhor local para fazer configurações iniciais primárias, inicializar padrões de Singleton e pegar referências pesadas de componentes (`GetComponent`).
*   **OnEnable():** Função disparada sempre que o GameObject é ativado na cena. Se você desligar e ligar o objeto várias vezes, o `OnEnable` vai rodar todas as vezes (diferente do `Start`, que só roda uma vez na vida do objeto).
*   **Start():** Função chamada automaticamente pela Unity apenas **uma única vez** no momento em que o objeto nasce. Excelente local para iniciar lógicas após o `Awake` ter preparado as dependências.
*   **Update():** Função chamada repetidamente a cada quadro (frame) desenhado na tela. Como a frequência varia de acordo com o FPS do computador, é ideal para capturar inputs (cliques do jogador) e timers básicos.
*   **FixedUpdate():** Função chamada em intervalos de tempo matematicamente fixos e regulares. **Regra de ouro:** Todo cálculo ou alteração que envolva **Física** (como mover um Rigidbody) deve obrigatoriamente ser feito aqui dentro.
*   **InvokeRepeating:** Função nativa para criar repetições temporizadas de forma fácil.
*   **Coroutines (IEnumerator e yield return):** Usadas para criar rotinas que executam ao longo do tempo ou precisam esperar (ex: piscar o jogador ao tomar dano, ou esperar o pavio da bomba explodir com `yield return new WaitForSeconds()`).
*   **DontDestroyOnLoad():** Um comando muito poderoso usado em Managers. Ele impede que o GameObject seja destruído quando o jogador troca de cena (ex: mantém a música tocando ou gerencia o status da rede entre as salas do jogo).

### Entradas (Inputs) e Gerenciamento
*   **Input.GetAxis("Horizontal"):** Captura comandos de movimento lateral do jogador. Ele reconhece automaticamente as setas Direita/Esquerda do teclado, as teclas A/D, ou o direcional de um controle analógico, retornando um valor gradual entre -1 (esquerda total) e 1 (direita total).
*   **SceneManager.LoadScene():** O comando principal do *SceneManager* para mudar de tela no jogo (ex: Carregar o menu principal, reiniciar a fase atual ao morrer, ou passar para o próximo nível).

### Eventos de Colisão e Gatilho (Triggers)
Estas funções são chamadas *automaticamente* pela engine de física (Physics 2D) quando colisores se encontram:
*   **OnCollisionEnter2D(Collision2D):** É disparado no exato momento que ocorre uma "batida" física real, ou seja, quando dois objetos sólidos se chocam e a engine calcula o impacto.
*   **OnCollisionExit2D(Collision2D):** É disparado no exato instante em que os objetos deixam de se tocar e se separam após uma batida contínua.
*   **OnTriggerEnter2D(Collider2D):** É disparado no momento em que um objeto adentra o espaço de um Collider marcado como **IsTrigger** (o sensor fantasma). Muito usado para identificar quando o jogador pisou em uma armadilha ou entrou na área de coleta de um power-up.
*   **Physics2D.OverlapBoxAll():** Muito útil, usado por exemplo no controle da explosão das bombas. Em vez de esperar alguém bater, o código "projeta" invisivelmente uma caixa em um local específico da fase e retorna uma lista de todos os Colliders que estão dentro daquela área naquele exato segundo.
*   **LayerMask (e Bitwise shifts):** Usado em conjunto com projeções físicas (como o OverlapBoxAll) para filtrar *o que* deve ser detectado (ex: a explosão só quer saber onde tem Caixas Destrutíveis, então ignoramos tudo que está na layer de cenário vazio).

---

## 🌐 6. Rede e Multiplayer (Photon PUN 2)

O BomberTeen utiliza a engine de rede **Photon PUN 2** para implementar a lógica de multiplayer e sincronização via internet. Muitos componentes tradicionais da Unity têm seus equivalentes no Photon.

*   **MonoBehaviourPunCallbacks:** Em vez de herdar apenas de `MonoBehaviour`, os scripts de rede herdam dessa classe. Ela libera acesso aos Callbacks (eventos automáticos) do Photon, permitindo reagir instantaneamente quando um evento ocorre, como `OnConnectedToMaster` ou `OnPlayerLeftRoom`.
*   **Gerenciamento de Sala (Connect / JoinRandomRoom / CreateRoom):** São funções primárias da API da classe estática `PhotonNetwork` utilizadas no `GestorDeRede` para autenticar os clientes nos servidores Master, buscar partidas automáticas em andamento ou iniciar novas instâncias de jogo.
*   **PhotonNetwork.Instantiate / PhotonNetwork.Destroy:** Substitutos diretos do `Instantiate` e `Destroy` originais da Unity. Eles garantem que, quando você cria uma bomba ou quebra uma parede no seu PC, esse mesmo objeto nascerá ou será apagado, ao mesmo tempo, na tela de **todos** os outros jogadores presentes na sala.
*   **photonView.IsMine:** Uma das checagens mais cruciais de segurança em multiplayer! Ela verifica se quem está rodando o script é o "Dono Local" (quem criou/tem autoridade sobre o objeto). Usada no `MovimentScript` para garantir que as setas do teclado do *Jogador 1* não controlem por acidente o boneco do *Jogador 2*.
*   **PhotonNetwork.IsMasterClient:** Avalia se o jogador da sua tela atual é o anfitrião (Host) da sala. Para evitar bugs (como caixas quebrando duas vezes), muitas vezes centralizamos lógicas críticas apenas no Master Client.
*   **[PunRPC] e photonView.RPC():** O grande responsável pela sincronização de dados (Remote Procedure Call)! Quando você marca uma função com a tag `[PunRPC]`, você pode utilizar `photonView.RPC("NomeDaFuncao", RpcTarget.All)` em qualquer lugar para ordenar que o jogo de *todos os jogadores na rede* execute aquela mesma função simultaneamente (Ex: Tocar o efeito sonoro de explosão na tela de todo mundo ou sincronizar o lado em que o personagem está olhando).

---

## 🔊 7. Sistema de Áudio

A Unity possui um sistema embutido simples, porém muito robusto, para trabalhar com efeitos sonoros (SFX) e músicas.

*   **AudioSource:** O "alto-falante". É o componente acoplado em um GameObject (como a bomba, o player ou no seu `AudioManager`) responsável por efetivamente emitir o arquivo de áudio.
*   **AudioListener:** O "ouvido". Geralmente fica **obrigatoriamente** anexado na *Main Camera*. Ele "escuta" os sons emitidos pelos *AudioSources* da cena e os manda para a saída de áudio do computador do jogador. (Dica de ouro: Ter mais de um AudioListener ativo na mesma cena causa alertas chatos na Unity).
