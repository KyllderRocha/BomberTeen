# 🚀 Plano de Melhorias e Backlog Técnico - BomberTeen

Com base na análise arquitetural dos scripts do projeto (como `MovimentScript`, `GameManager`, `BombController`, `GestorDeRede`, etc.), levantei uma série de melhorias fundamentais.

As tarefas abaixo estão classificadas por tipo e ordenadas por prioridade técnica. O objetivo é guiar as próximas fases do desenvolvimento focando em **Clean Code, SOLID, Escalabilidade** e a finalização de lógicas do jogo.

---

## 🔴 Alta Prioridade (Arquitetura, SOLID e Performance)

### 1. Refatoração do `MovimentScript.cs` (Falta de Single Responsibility Principle - SRP)
*   **Problema:** O script atualmente possui responsabilidades demais ("God Class" ou classe deus). Ele gerencia captura de input, motor físico, sincronização de animações pela rede (RPCs), verificação de dano (`OnTriggerEnter2D`) e ainda toca a sequência de morte e final de jogo.
*   **Solução:** Quebrar esse script em componentes menores e altamente coesos:
    *   `PlayerInput`: Apenas captura teclas.
    *   `PlayerMovement`: Recebe a direção e aplica a força no Rigidbody.
    *   `PlayerHealth`: Cuida das colisões com explosões e animação de morte.
    *   `PlayerAnimatorSync`: Escuta o movimento e gerencia os `changeSprite`.

### 2. Remoção do Forte Acoplamento (Inversão de Dependência)
*   **Problema:** O uso extensivo e fixo de Singletons (`GameManager.Instancia.CheckWinState()`, `MenuManager.instancia.GameOver()`) acopla os sistemas. O script do jogador não deveria precisar "conhecer" o GameManager para poder morrer.
*   **Solução:** Implementar o padrão **Observer (Eventos)**. Em vez do jogador chamar o manager, ele deveria apenas gritar pro jogo: `"Eu morri!"` (Ex: `public static event Action OnPlayerDied;`). O GameManager que se inscreva (listen) nesse evento e, quando escutar, faça a sua validação, tornando os sistemas 100% independentes.

### 3. Otimização de Garbage Collection (Falta de Cache em Loops)
*   **Problema:** No método `Update` de `Bomb.cs` (que roda cerca de 60 vezes por segundo), estão ocorrendo instâncias constantes de classes pesadas (`new ContactFilter2D()` e `new Collider2D[10]`). Multiplique isso pelo número de bombas e haverá picos de travamentos (Garbage Collection Spikes).
*   **Solução:** Mover essas alocações para as variáveis globais da classe e utilizar a função super-otimizada `Physics2D.OverlapBoxNonAlloc()`, reaproveitando a mesma lista na memória.

---

## 🟠 Prioridade Média (Redes e Segurança)

### 4. "Client Authority" vs "Server Authority"
*   **Problema:** Em jogos multiplayer, nunca confie no cliente (jogador). Hoje, o script da máquina do jogador dita se ele morreu ou não, além de decidir se destruiu uma caixa no `BombController`. Isso abre brechas absurdas para hackers (cheaters) apenas mudarem as variáveis locais para não tomarem dano.
*   **Solução:** Refatorar as validações críticas (morte, pegar itens, quebrar blocos) para serem decididas única e exclusivamente pelo **Master Client** (o "Servidor" da sala).

### 5. Eliminação de "Magic Strings"
*   **Problema:** O código está cheio de chamadas em texto, como `PlaySFX("Explosion")` ou `LayerMask.NameToLayer("Explosion")`. Se houver um simples erro de digitação, a engine compila perfeitamente, mas o jogo "crasha" misteriosamente quando o jogador joga a bomba.
*   **Solução:** Criar uma classe estática (ex: `GameConstants.cs`) para armazenar todas as strings imutáveis. (Ex: `public const string LAYER_EXPLOSION = "Explosion";`).

### 6. Atualização da Engine de Rede
*   **Problema (Fim de Vida):** O Photon PUN 2 é excelente, mas infelizmente entrou na fase "Legado" de suporte oficial.
*   **Solução:** Planejar a longo prazo uma migração gradual da lógica do `GestorDeRede` para os pacotes mais novos focados na Unity 6, como o **Photon Fusion** ou o sistema nativo **Unity Netcode for GameObjects (NGO)**.

---

## 🟡 Prioridade Média (Gameplay e Finalizações)

### 7. Inteligência Artificial (Desenvolvimento de Bots)
*   **Problema:** Não há implementação clara para completar as vagas caso uma sala online não encha com 4 jogadores reais.
*   **Solução:**
    *   Habilitar o **NavMesh 2D** da Unity no mapa.
    *   Criar um componente `BotController` usando uma Máquina de Estados Finita (FSM).
    *   *Estados sugeridos:* **Buscar Caixa** (acha a parede destrutível mais próxima), **Atacar/Plantar** (solta a bomba), e o estado de **Fuga Segura** (mede o `explosionRadius` para fugir da fumaça).

### 8. Gestão de Status via Scriptable Objects
*   **Problema:** Valores como *Tempo do Pavio*, *Tamanho do Raio* e *Quantidade de Bombas* ficam presos a variáveis comuns dentro dos scripts, dificultando a equipe de Game Design tentar balancear o jogo.
*   **Solução:** Transpor todos esses status (`bombFuseTime`, `speed`, etc.) para um **Scriptable Object**. Assim, todos os Power-Ups pegos via `ItemPickup.cs` apenas atualizam os status contidos dentro desse "arquivo de configuração", mantendo o código puro e flexível.

### 9. Tratamento de Empates Múltiplos (Draw Condition)
*   **Problema:** O `CheckWinState()` confere se sobrou `1 ou menos` jogadores. No entanto, se o Jogador 1 e o Jogador 2 ficarem vivos para o final, e se explodirem simultaneamente, não existe fluxo visual de empate, podendo gerar soft-locks.
*   **Solução:** Criar lógica para checar mortes síncronas num espaço de milissegundos e engatilhar a tela `DrawScreen` ao invés da tela padrão de Vitória.

---

## 🟢 Prioridade Baixa (Polimento e Qualidade de Vida)

### 10. Implementação de Namespaces e Asmdefs
*   **Problema:** A maioria dos scripts (`Bomb.cs`, `GameManager.cs`) habita o namespace principal do C# vazio. Em projetos de longo prazo, classes com nomes comuns vão começar a dar conflito (ex: criar outro tipo de bomba, ou outro gestor).
*   **Solução:** Agrupar fisicamente usando as chaves `namespace BomberTeen.Core`, `namespace BomberTeen.Network`, etc. Além disso, definir arquivos `.asmdef` por pasta garantirá que quando você editar a UI, o código pesado do servidor não precisará ser recompilado pela Unity, poupando minutos diários.

### 11. Novo Input System da Unity
*   **Problema:** O movimento usa o sistema antigo (`Input.GetKey(KeyCode.W)`). Isso torna quase inviável jogar com controles de videogame nativamente sem lotar o código com centenas de "Ifs e Elses".
*   **Solução:** Migrar os comandos (Up, Down, Left, Right e Plant) para o novo pacote **Input System**. Ele dissocia o código de "qual botão foi apertado", permitindo você dizer apenas: `Quando o jogador executar a Ação de Movimento, ande`, e a engine cuida se veio de um teclado, manete de Xbox ou celular touch.
