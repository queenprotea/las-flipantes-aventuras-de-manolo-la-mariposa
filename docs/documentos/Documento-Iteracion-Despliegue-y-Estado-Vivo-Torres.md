# Iteración de diseño: despliegue del sistema y organización del estado vivo — Juego de Torres

**Versión 1.0 · 21 sep 2026.**

---

## 1. Objetivo de la iteración

Decidir en cuántos nodos se despliega el sistema, cuántos procesos de servidor existen y cómo la
estructura sostiene el aislamiento entre partidas simultáneas.

Quedan fuera del objetivo: el diseño interno de cada módulo, el reparto de los veintiocho casos de
uso en operaciones del contrato, las pantallas y las decisiones que dependen del conflicto `C-2`
sobre la seguridad del transporte.

---

## 2. Entradas consideradas

### 2.1 Estructura de la que se parte

| Elemento | Responsabilidad |
|---|---|
| `Game.Client` | Interacción y representación. Envía intenciones, no determina resultados |
| Ensamblado de contratos compartido sobre `netstandard2.0` | Operaciones, contrato de callback y tipos que cruzan la frontera |
| `Game.Services` | Autoridad sobre el estado vivo, reloj del turno, latido, lectura de configuración y publicación de cambios |
| `Game.Domain` | Reglas, puntuación, geometría y desempate. No depende de ninguna otra capa |
| `Game.Persistence` | Entidades, catálogos y documento de estado de la partida en las fronteras de turno |

El estado vivo reside en la memoria del proceso servidor y la persistencia guarda fronteras de
turno. La notificación de cambios se hace por callbacks duplex.

### 2.2 Documentos

`Base-Oficial-STK-CON-DEP-Torres.md` v4.1, `Escenarios-Arquitectonicos-Torres.md` v1.0,
`Arbol-de-Utilidad-Torres.md` v1.0, `Documento-Base-Consolidada-Torres.md`, `STACK.md`,
`Estandar-de-codificacion-v7.md` §2, `Analisis-Prueba-Myra-Torres.md` y
`Documento-Internacionalizacion-Torres.md`.

---

## 3. Clasificación de las afirmaciones relevantes

Antes de defender una decisión se comprueba que exista evidencia para tratar cada afirmación como
restricción. **Una falsa restricción cierra el diseño sin motivo.**

| Afirmación | Clasificación | Evidencia y comprobación |
|---|---|---|
| La comunicación es WCF duplex sobre net.tcp | **Impuesta** | `RES-02`, `STACK §2.2`. Exigida por STK-03 |
| Está prohibido el stack web y los WebSockets | **Impuesta** | `RES-03`, `STACK §2.3`. Nombra SignalR, gRPC y REST |
| Cuatro capas con dirección fija de dependencias | **Impuesta** | `RES-05`, `E 2.1`, `E 2.2`. Verificable por las referencias de los `.csproj` |
| El cliente no decide resultados | **Impuesta** | `RES-05` |
| El motor de base de datos es PostgreSQL | **Elegida** | `DR-33`. Descarta a sabiendas el entregable que se ejecutaba con doble clic |
| `SecurityMode.None` en el `NetTcpBinding` | **Elegida, con conflicto abierto** | `RES-09`, `C-2`, `TEN-I`. **Aparece como restricción y no procede de STK-03, sino de `ORG-03`.** No cierra el despliegue; determina qué viaja sin cifrar |
| `DesktopGL` y no `WindowsDX` | **Impuesta por condición organizacional** | `RES-08`, `ORG-03`. Es verdadera para este equipo y por ese motivo |
| El sistema no limita las partidas simultáneas | **Elegida** | `DR-10`. Es una decisión de diseño, no una medida de capacidad |
| Con 5 partidas en curso, ninguna altera el estado, el reloj ni el resultado de otra | **Elegida como invariante de verificación** | `DR-31`. Las 5 no son un límite del sistema: son la cifra con la que se verifica |
| Cada integrante debe poder correr servidor y clientes en su máquina | **Impuesta por condición organizacional** | `ORG-04`, `OBJ-02`, `STACK §2.6`, que la señala como la restricción que más decisiones fuerza |
| No hay requisitos de latencia ni de carga | **Ausencia comprobada** | Base §7. El único dato de rendimiento en las fuentes es una justificación de estilo sobre el coste de las excepciones |
| La pérdida de conexión se detecta por latido | **Elegida** | Base consolidada §4.9. El mecanismo está decidido; sus cifras no |
| Un tercer nivel físico mejoraría la seguridad | **Falsa restricción** | Ninguna fuente lo pide. Se examina en el apartado 6 |
| STK-03 aceptará CoreWCF | **Supuesto** | `ASM-02`, `C-4`. No depende de STK-02 |
| El estado vivo cabe en la memoria de un proceso | **Supuesto** | `TEN-G`. Nadie lo ha examinado |

---

## 4. Drivers seleccionados y su peso

| Peso | Drivers | Motivo |
|---|---|---|
| **Decisivo** | `RES-02`, `RES-03`, `RES-05`, `ORG-03`, `ORG-04`, `DR-33` | Restricciones y condiciones que eliminan alternativas de despliegue |
| **Decisivo** | **CON-05** con su invariante `DR-31`, y **CON-01 en su variante de caída del servidor** | Afectan la estructura global: dónde vive el estado compartido y qué ocurre al reiniciar |
| **Alto** | `ORG-01`, `ORG-05`, `TEN-H`, `TEN-G` | Fijan cuánta complejidad puede sostenerse y cuál es el riesgo del estado en memoria |
| **Medio** | **F-1**, **T-1** | Atendidos estructuralmente en la iteración anterior. Aquí solo se comprueba que el despliegue no los rompa |
| **Bajo para estas decisiones** | **M-1**, **L-1**, CON-06, CON-10 | M-1 se satisface con una fila de una tabla en cualquier despliegue; L-1 no cambia ninguna estructura; CON-06 y CON-10 dependen de `C-2` |
| **Funcionalidad primaria** | CU-25, CU-19, CU-24, CU-26, CU-27, CU-13, CU-01 | Definen qué sistema se construye: un servidor que sostiene partidas vivas con varios clientes conectados a la vez |

---

## 5. Elementos que se refinan

1. **El sistema como conjunto desplegable**, que no tenía decisión.
2. **`Game.Services`**, que recibió la custodia del estado vivo sin que se dijera cómo lo organiza
   para sostener CON-05.
3. **El ensamblado de contratos**, del que se decide su forma.

---

## 6. Alternativas de despliegue

### 6.1 Qué se decide

El motor de base de datos **ya es un proceso aparte** y en macOS arm64 corre en un contenedor
`STACK §5`, `DR-33`. Además, la frontera del sistema lo deja **fuera**: es una dependencia externa
`DEP-E7`, no un artefacto del sistema.

La decisión no es si la base está separada, sino si **`Game.Persistence` se convierte en un elemento
desplegable propio, con una frontera remota entre ella y `Game.Services`**.

### 6.2 Alternativa 1 · Dos niveles

Cliente y presentación en un nodo; servidor con servicios de negocio, acceso a datos y base en otro.

```
Nodo Cliente (1..n)                 Nodo Servidor (1)                Motor de base (externo)
┌────────────────────┐   net.tcp   ┌───────────────────────────┐    ┌──────────────┐
│ Game.Client        │◄──duplex───►│ Host CoreWCF              │    │ PostgreSQL   │
│ Contratos          │             │ Game.Services             │───►│              │
│ System.ServiceModel│             │ Game.Domain               │    └──────────────┘
└────────────────────┘             │ Game.Persistence          │
                                   │ archivos de avatar        │
                                   └───────────────────────────┘
```

### 6.3 Alternativa 2 · Tres niveles

Cliente y presentación; servidor con servicios de negocio; persistencia, acceso a datos y base como
nivel propio.

```
Nodo Cliente (1..n)      Nodo Servicios (1)          Nodo Persistencia (1)      Motor de base
┌──────────────┐ net.tcp ┌──────────────────┐        ┌──────────────────┐       ┌───────────┐
│ Game.Client  │◄duplex─►│ Game.Services    │◄──────►│ Game.Persistence │──────►│PostgreSQL │
└──────────────┘         │ Game.Domain      │ segundo└──────────────────┘       └───────────┘
                         └──────────────────┘  salto
```

### 6.4 Comparación

| Criterio | Dos niveles | Tres niveles |
|---|---|---|
| **Aislamiento de responsabilidades** | Lo da `RES-05`, y se comprueba mirando las referencias de los `.csproj`, que es la medida 2 de T-1 | No añade aislamiento. Separar procesos no hace más estricta una dependencia que el compilador ya impide |
| **Frontera de confianza** | La única frontera de confianza real es cliente–servidor, porque el cliente corre en la máquina del jugador | **No añade frontera:** el cliente no alcanza la persistencia en ninguna de las dos alternativas |
| **Despliegue independiente** | Suficiente: hay un solo operador, que es STK-02 `ORG-02` | Sería una ventaja si alguien desplegara o administrara la persistencia por separado. Nadie lo hace |
| **Escalar o administrar por separado** | No hace falta: no hay requisitos de latencia ni de carga en ninguna fuente | La presión registrada es `TEN-G`, de memoria del proceso servidor. Un nivel de persistencia aparte no la alivia: el estado que crece está en memoria, no en base |
| **Cumplimiento de una restricción** | Ninguna `RES-` exige separación física | Ninguna `RES-` la impone |
| **Comunicación remota** | Una sola frontera remota, ya verificada `STACK §6` | Un segundo salto remoto por construir. Con `RES-03` fuera de juego, sería otro canal net.tcp con su contrato, su modo de fallo y su serialización |
| **Complejidad añadida** | La mínima compatible con las restricciones | Contra `ORG-01`, `ORG-05` y `TEN-H`: dos personas, cinco papeles, catorce semanas |
| **`ORG-04`, levantar todo en la propia máquina** | Dos procesos propios más el contenedor de la base | Tres procesos propios más el contenedor. Un elemento más que coordinar cada vez que alguien prueba |
| **Modos de fallo** | El cliente cae, o el servidor cae. Los dos están especificados en CON-01 | Aparece un tercero, la persistencia inalcanzable con el servidor en pie, que **ninguna fuente del proyecto ha especificado** |

---

## 7. Decisiones de la iteración

### Decisión 1 · Despliegue en dos niveles

El sistema se despliega en un **nodo cliente** por jugador, con `Game.Client` y los contratos, y un
**nodo servidor** único que aloja el host de CoreWCF con `Game.Services`, `Game.Domain` y
`Game.Persistence`. El motor de PostgreSQL es un elemento externo a la frontera, desplegado como
proceso propio y en contenedor sobre macOS arm64. El sistema de archivos donde viven los avatares
pertenece al nodo servidor `DEP-E10`.

**Justificación.** El tercer nivel no aporta ninguno de los beneficios que justificarían su coste:
el aislamiento ya lo da `RES-05` y es verificable por compilación, la única frontera de confianza es
cliente–servidor, no hay operación separada ni requisitos de carga, y ninguna restricción lo exige.
Su coste sí es comprobable contra drivers existentes: un segundo salto remoto que habría que diseñar
sobre net.tcp por exclusión de `RES-03`, un modo de fallo nuevo sin especificar, y un proceso más
que levantar en cada máquina, contra `ORG-04` y `OBJ-02`.

La decisión no se toma por ser el patrón más común: el argumento habitual a favor del tercer nivel
—proteger la base de datos— no aplica aquí, porque el cliente no tiene acceso a la persistencia en
ninguna de las dos alternativas.

Con `SecurityMode.None` `RES-09`, lo que cruza la única frontera remota —credencial, mensajes de
chat y pase de asiento del invitado— viaja sin cifrar. El despliegue en dos niveles no modifica ese
punto, que es el conflicto `C-2`.

### Decisión 2 · Un único proceso de servidor

Un solo proceso aloja todo el estado vivo y atiende todos los canales duplex.

| Alternativa | Efecto sobre CON-05 | Efecto sobre el resto |
|---|---|---|
| Un proceso por partida | Aislamiento fuerte por proceso del sistema operativo | El canal duplex del cliente se establece contra un extremo: habría que enrutarlo o reconectarlo al proceso correcto al empezar la partida. Cuentas, salas, amistades y ranking son compartidos y tendrían que vivir fuera. Complejidad muy superior, sin ningún driver que la pida |
| Un proceso con el estado particionado | Aislamiento por estructura de datos | El canal ya establecido sirve para sala y para partida, y `MatchSetupService` configura las partidas dentro del mismo proceso `DR-11` |

**Coste aceptado.** Mantener todo el estado vivo en un proceso es la situación que describe
`TEN-G`: sin límite de partidas `DR-10`, con salas, chat y ranking de sala en memoria y con partidas
caídas retenidas tres minutos. La tensión no se cierra con esta decisión, y `ORG-04` la agrava
porque ese proceso corre en el portátil de cada integrante.

### Decisión 3 · Estado vivo particionado por partida y por sala

No hay estado mutable compartido entre partidas. Las operaciones sobre una misma partida se aplican
de una en una, de modo que dos acciones concurrentes nunca observen un estado intermedio. Lo único
común entre partidas es lo que ya es común por diseño: las cuentas, los catálogos de parámetros y el
registro de eventos.

**Justificación.** Es la respuesta estructural a CON-05, que exige que lo que ocurre en una partida
no altere el estado, el reloj ni el resultado de otra. Su medida no es una cifra de capacidad sino
un invariante, verificado con 5 partidas en curso `DR-31`, y la superficie sobre la que hay que
demostrarlo incluye el estado en memoria, no solo el de la base `D-26`, `D-29`. Si no hay estructura
compartida entre partidas, el invariante se comprueba ejercitando cinco partidas a la vez y
observando que ninguna ve el estado de otra.

### Decisión 4 · Un único ensamblado de contratos sobre `netstandard2.0`

Referenciado por el nodo cliente y por el nodo servidor, declara las operaciones, el contrato de
callback y los tipos que cruzan la frontera. Está respaldado por `DEP-E11` y `STACK §3`: sin un solo
ensamblado, cada cambio de contrato desincroniza los dos lados.

### Decisión 5 · Los fallos que el servidor comunica al cliente viajan como código

**Justificación.** El idioma existe solo en el cliente: el servidor no sabe qué idioma usa cada
jugador, de modo que un error que viajara como texto llegaría siempre en un solo idioma. El
documento de internacionalización lo enuncia y advierte que toca los contratos compartidos, por lo
que debe decidirse antes de escribirlos. Consecuencia estructural: el ensamblado de contratos
declara un catálogo de códigos, y los recursos de idioma no entran en él.

Queda pendiente de ratificación por STK-02, que es quien cierra las decisiones del proyecto.

### Decisión 6 · El cliente traslada los callbacks al hilo del juego

El cliente recibe los callbacks en un hilo del canal y los traslada al hilo del juego mediante una
cola que el ciclo de actualización vacía. Ninguna estructura de la interfaz se modifica desde el
hilo del canal.

**Justificación.** Es consecuencia de combinar dos componentes externos ya elegidos: CoreWCF llama
de vuelta desde un hilo del canal y MonoGame dibuja en el hilo del juego. El análisis de la prueba
de Myra lo identifica como el riesgo real de adoptar una biblioteca de interfaz retenida y señala
que condiciona los contratos compartidos. La decisión es independiente de si finalmente se adopta
Myra: con la caja de herramientas actual el problema no se manifiesta, pero la cola es lo que
permite adoptarla después sin rehacer el cliente.

### Decisión 7 · El trabajo de arranque pertenece al host del servidor

Al arrancar, y antes de aceptar clientes, el proceso servidor cierra las salas que quedaran abiertas
`DR-18a` y busca las partidas en curso para reanudarlas.

**Justificación.** Ninguna de las dos acciones tiene actor humano y ninguna puede ejecutarse a
petición de un cliente, porque ocurren cuando todavía no hay clientes. Ubicarlas en el arranque es
lo que hace realizable la respuesta de CON-01 ante la caída del servidor: al arrancar, el servidor
busca las partidas en curso y las reanuda cuando han vuelto al menos dos jugadores, con un plazo de
espera de tres minutos.

---

## 8. Responsabilidades asignadas

### Nodo cliente

Las de `Game.Client`, más: establecer el canal duplex, emitir el latido con el periodo configurado y
trasladar los callbacks al hilo del juego.

### Nodo servidor

| Responsabilidad | Elemento |
|---|---|
| Abrir el extremo net.tcp, aceptar sesiones duplex y cerrarlas | Host de CoreWCF, sobre el host genérico de ASP.NET Core |
| Cerrar las salas abiertas y reanudar las partidas en curso al arrancar, antes de aceptar clientes | Host de CoreWCF |
| Custodiar el estado vivo particionado y aplicar las operaciones de una en una por partida | `Game.Services` |
| Vigilar los latidos, declarar la pérdida de conexión e iniciar la ventana de reconexión y el marcado de inactividad | `Game.Services` |
| Configurar la partida que va a empezar leyendo los catálogos | `MatchSetupService`, en `Game.Services` `DR-11` |
| Decidir legalidad, puntuación, colocador del rey, ganador y desempate | `Game.Domain` |
| Escribir la frontera de turno y eliminar el estado al terminar | `Game.Persistence`, por orden de `Game.Services` |

`MatchSetupService` es el único componente cuyo nombre fija una fuente del proyecto. Esta iteración
asigna responsabilidades, no nombres.

---

## 9. Estructura resultante

### 9.1 Vista de despliegue

```
   ┌──────────────────────────┐              ┌───────────────────────────────────────┐
   │  Nodo Cliente  (1..n)    │   net.tcp    │       Nodo Servidor  (1)              │
   │  un proceso por jugador  │   duplex     │                                       │
   │                          │◄────────────►│  Host CoreWCF                         │
   │  Game.Client (MonoGame)  │ SecurityMode │   └─ al arrancar: cierra salas y      │
   │  Contratos compartidos   │    .None     │      reanuda partidas en curso        │
   │  latido y cola de        │              │                                       │
   │  callbacks               │              │  Game.Services — estado vivo          │
   └──────────────────────────┘              │    particionado por partida y sala    │
                                             │  Game.Domain                          │
                                             │  Game.Persistence                     │
                                             │  archivos de avatar                   │
                                             └────────────┬──────────────────────────┘
                                                          │
                          ┌───────────────────────────────┴───────┐
                          ▼                                       ▼
             ┌──────────────────────┐                ┌────────────────────────┐
             │ PostgreSQL           │                │ Canal de correo        │
             │ proceso propio,      │                │ externo a la frontera  │
             │ contenedor en macOS  │                │                        │
             │ externo a la frontera│                └────────────────────────┘
             └──────────────────────┘
```

En desarrollo, por `ORG-04`, los tres se ejecutan en el portátil de cada integrante: uno o varios
clientes, el servidor y el contenedor de PostgreSQL. `STACK §6` verificó esa configuración en macOS
arm64; la ejecución en Linux, la ejecución en Windows y la conexión entre dos máquinas distintas
constan como no verificadas.

### 9.2 Refinamiento de la vista de módulos

- El **host de CoreWCF** es un elemento con responsabilidad propia dentro del nodo servidor,
  distinto de `Game.Services`: el ciclo de vida del proceso es suyo.
- **`Game.Services`** se organiza en una partición de estado vivo por partida y por sala, con
  aplicación serializada de operaciones por partida.
- El **ensamblado de contratos** incorpora un catálogo de códigos de fallo y no lleva recursos de
  idioma.
- El **cliente** incorpora la cola que traslada los callbacks entre el hilo del canal y el del
  juego.

---

## 10. Revisión del resultado

| Driver | Cobertura |
|---|---|
| **CON-05** aislamiento, invariante de 5 partidas | **Cubierto estructuralmente** por la decisión 3. La prueba que lo ejercita está por escribir |
| **CON-01**, caída del servidor | **Cubierto.** La decisión 7 ubica la reanudación en el arranque del host |
| **D-1** disponibilidad | **Parcial.** El latido queda ubicado en cliente y servidor; sus cifras no están fijadas |
| **F-1** y **T-1** | **No se rompen.** Un solo proceso mantiene el punto único de determinación, y el dominio sigue sin referencias a otras capas |
| **M-1** y **L-1** | Indiferentes al despliegue; se siguen cumpliendo |
| `ORG-04` y `OBJ-02` | **Cubiertos.** Dos procesos propios y un contenedor por máquina, que es lo mínimo compatible con `DR-33` |
| `RES-02`, `RES-03`, `RES-05` | **Cubiertos.** Una sola frontera remota, sobre net.tcp duplex, sin introducir ningún protocolo prohibido |
| CON-06 y CON-10 | **No cubiertos.** El conflicto `C-2` sigue sin definir; el despliegue lo deja igual |
| `TEN-G` | **No se cierra.** La decisión 2 la asume de forma explícita |
| Funcionalidad primaria | Los siete casos de uso seleccionados tienen nodo, módulo y responsabilidad. No tienen operaciones del contrato asignadas |

El objetivo de la iteración se alcanza: hay una decisión de despliegue justificada contra drivers,
una respuesta estructural a CON-05 y las decisiones de contrato y de cliente que condicionaban la
escritura del ensamblado compartido.
