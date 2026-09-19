# Historias de usuario — Las flipantes aventuras de Manolo la mariposa (Torres)

**Documento:** `Documento-Historias-de-Usuario-Torres.md` · versión 3.0 · 10 sep 2026
**Qué es:** el complemento de `Documento-Casos-de-Uso-Torres.md`. Recoge los comportamientos del
sistema que **deben existir y que ningún caso de uso describe**, y los cubre con historias de
usuario en lugar de inventarles un actor que no tienen.

**Qué no es:** no es una lista de deseos ni una ampliación del alcance. **Todas las historias de
este documento se apoyan en una funcionalidad ya acordada** —`FC-xx` de la base consolidada— o en
una regla ya cerrada. No hay ninguna que introduzca una capacidad nueva.

---

## 1. El concepto, antes de aplicarlo

### 1.1 Qué distingue a un caso de uso de una historia de usuario

| | **Caso de uso** | **Historia de usuario** |
|---|---|---|
| **Qué describe** | La **interacción** entre un actor y el sistema para alcanzar un objetivo | La **necesidad** de alguien, sin describir la interacción |
| **Qué exige** | Actor, disparador, flujo paso a paso, alternos, excepciones, postcondiciones | Rol, deseo, beneficio y criterios de aceptación |
| **Cuándo sirve** | Cuando alguien **usa** el sistema y se puede narrar cómo | Cuando el valor existe pero **nadie ejecuta nada**, o cuando el comportamiento es transversal |
| **Quién lo dispara** | Siempre un actor | El sistema, el reloj, un arranque, o una condición del entorno |

### 1.2 La regla que se aplicó aquí

Un comportamiento merece **historia de usuario** y no caso de uso cuando se cumplen las tres:

1. **No hay actor que lo persiga.** Lo dispara el servidor, el vencimiento de un plazo o el
   arranque del proceso. Darle un actor obligaría a inventar uno —y `D-31` ya descartó el rol de
   administración, así que no hay a quién atribuírselo.
2. **No hay interacción que narrar.** No hay ventana, ni opción que accionar, ni pasos que
   alternen entre el actor y el sistema. Un flujo normal quedaría reducido a una lista de cosas
   que hace el sistema solo.
3. **Aun así hay alguien que recibe el valor.** Si no lo hubiera, no sería ni historia: sería una
   decisión de diseño.

Y un comportamiento **no merece historia** si ya está descrito dentro del flujo de un caso de
uso. Duplicarlo produciría dos fuentes para la misma regla, que es exactamente lo que esta fase
intenta evitar.

---

## 2. Qué quedó cubierto y qué no

La base consolidada declara **doce comportamientos del sistema**, `S-1` a `S-12`, y dice de ellos
que «modelarlos como casos de uso o como comportamiento del sistema es una decisión del equipo;
**cubrirlos no es opcional**». Se revisó uno a uno contra las descripciones ya escritas.

| | Comportamiento | ¿Está descrito? | Dónde |
|---|---|---|---|
| `S-2` | Cierre del turno por agotarse los 90 s | **Sí** | CU-25, FA09, y el cierre es paso 5 de su flujo normal |
| `S-3` | Cierre de la ronda | **Sí** | CU-25, FA10, y CU-24, FA03, para la colocación del rey |
| `S-5` | Retiro del jugador que agota su ventana de reconexión | **Sí** | CU-26, FA04, y CU-28 para el abandono voluntario |
| `S-6` | Reanudación tras una caída, o cierre si nadie vuelve | **Sí** | CU-27, flujo completo y FA03/FA04 |
| `S-8` | Aplicación de la sanción al alcanzarse el umbral | **Sí** | CU-23, FA04 |
| `S-1` | Cierre de las salas abiertas al arrancar el servidor | **No** | → `HU-01` |
| `S-10` | Marcado de inactividad y traspaso del anfitrión | **No** | → `HU-02` |
| `S-11` | Retirada de la sala del inactivo que agota el plazo | **No** | → `HU-03` |
| `S-4` | Fin de la partida y registro del resultado | **Apenas** | → `HU-04` |
| `S-7` | Actualización del ranking de la sala | **No** | → `HU-05` |
| `S-9` | Caducidad de la prohibición temporal | **No** | → `HU-06` |
| `S-12` | Registro de los eventos del sistema | **No** | → `HU-07` |

**Sobre `S-4`.** Es el único que estaba a medias, y por eso se trata aparte. CU-25 FA10 lo
despacha en una línea —«el sistema termina la partida y registra su resultado»— y CU-26 FA04 y
CU-27 FA03/FA04 nombran los tres motivos de fin. Pero **nadie describe el cálculo del resultado**
—la suma final, el punto por cada carta no utilizada, el desempate— ni **qué se escribe** ni
**qué ve el jugador**. El prototipo ya tiene esa pantalla, `PT-24`, y ningún caso de uso la
alcanza.

**Un comportamiento más, fuera de la lista `S-`.** La base fija en §4.2 que «la opción **Salir**
del menú principal **cierra la aplicación**» `OB-15`. No tiene `FC-xx`, no es ninguna de las doce
situaciones y **ningún caso de uso la menciona**. Se cubre con `HU-08`.

---

## 3. Las historias

Las ocho siguen el mismo formato: **qué queda sin cubrir**, **por qué no debe ser un caso de
uso**, **la historia** y **sus criterios de aceptación**, con la funcionalidad real que cubre.

---

### HU-01 · Las salas no sobreviven al arranque del servidor

**Qué queda sin cubrir.** Una sala vive en dos sitios: una fila en la base de datos y su estado
en la memoria del servidor —jugadores, anfitrión, colores, chat y ranking—. Cuando el servidor
cae, **la memoria se pierde y la fila se queda con su fecha de cierre vacía**. Al arrancar de
nuevo quedan filas de salas que ya no existen: su código sigue ocupado, y sus invitaciones
pendientes apuntan a un sitio al que ya no se puede entrar. `DR-18a` manda cerrarlas, y ningún
caso de uso lo hace.

**Por qué no es un caso de uso.** No lo pide nadie y no ocurre por jugador, sino **una vez por
arranque del proceso**. No hay ventana, ni opción, ni actor: el único candidato sería un
administrador, y `D-31` descartó ese rol. En la versión anterior de las descripciones esto estaba
metido como paso 2 del flujo de CU-27, dentro del caso de un jugador que vuelve, lo cual hacía
creer que se ejecutaba una vez por cada jugador que se reconecta.

> **Como** jugador que vuelve a entrar después de que el servidor se haya reiniciado,
> **quiero** que no queden salas fantasma de la ejecución anterior,
> **para** que sus códigos vuelvan a estar libres y no se me ofrezcan invitaciones a salas a las
> que ya no se puede entrar.

**Criterios de aceptación**

1. **Dado** que al arrancar existen salas con la fecha de cierre vacía, **cuando** el servidor
   arranca, **entonces** el sistema les escribe su fecha de cierre.
2. **Entonces** el código de cada una de esas salas queda libre para volver a sortearse.
3. **Entonces** sus invitaciones pendientes quedan eliminadas, de modo que ningún jugador vea en
   su bandeja una invitación a una sala inexistente.
4. **Entonces** ninguna sala se recrea en memoria: no vuelven ni sus jugadores, ni su anfitrión,
   ni su chat, ni su ranking.
5. **Y** las partidas terminadas que referencian esas salas **se conservan**: la sala se cierra,
   no se borra.

**Cubre:** `S-1` · **Se apoya en:** `DR-18a`, `D-23`, `D-24`, `P-32` · **Afecta a:** CU-27
(POST-5) y CU-17 (FA03, la invitación cuya sala ya no existe).

---

### HU-02 · El que pierde la conexión queda marcado, y la sala no se queda sin anfitrión

**Qué queda sin cubrir.** Las descripciones **usan** el estado de inactividad sin decir nunca
cómo se llega a él: CU-13 muestra la marca de inactivo en la lista de jugadores, CU-19 FA02 los
excluye de la partida y CU-21 FA03 expulsa a uno. Pero ningún flujo describe **la transición**
—latidos perdidos → marcado inactivo → traspaso del anfitrión— ni la vuelta a activo al
reconectar.

**Por qué no es un caso de uso.** El disparo es **la ausencia de una señal**. El jugador afectado
no está: está desconectado, y por definición no persigue ningún objetivo. Los demás tampoco
accionan nada; solo ven cambiar la lista. Un caso de uso necesitaría un actor que aquí no existe.

> **Como** jugador que está en una sala,
> **quiero** que el sistema marque como inactivo a quien pierde la conexión y pase la condición
> de anfitrión al siguiente si era él quien la tenía,
> **para** saber a quién estamos esperando y que la sala no se quede sin nadie que pueda iniciar
> la partida.

**Criterios de aceptación**

1. **Dado** un jugador dentro de una sala, **cuando** el servidor deja de recibir sus latidos
   durante el número de latidos fijado, **entonces** lo marca como **inactivo** y lo muestra así
   en la lista de jugadores de todos los que están dentro.
2. **Entonces** ese jugador **sigue en la sala y sigue ocupando su plaza**: la sala nunca
   contiene más de cuatro personas, inactivos incluidos.
3. **Entonces** ese jugador **no entra a ninguna partida** que se inicie mientras siga inactivo,
   y **no cuenta** para el mínimo de dos ni para el máximo de cuatro jugadores de la partida.
4. **Dado** que el jugador marcado era el anfitrión, **entonces** la condición pasa **al
   siguiente jugador en el orden en que están guardados**, sin más criterio, y los demás ven
   cambiar la marca «Anfitrión».
5. **Cuando** ese jugador recupera la conexión, **entonces** vuelve a estar activo **sin nada que
   recuperar**, porque no se había perdido nada suyo; **y no recupera la condición de anfitrión**
   si ya la había heredado otro.
6. **Dado** que el jugador está **dentro de una partida en curso**, **entonces** esto no se le
   aplica: rige la ventana de reconexión de la partida, descrita en CU-26.

**Cubre:** `S-10`, `FC-42` · **Se apoya en:** `OB-46`, `OB-50` (fase 1), `OB-10`, `P-36`, §4.4 y
§4.9 de la base · **Afecta a:** CU-13 (paso 5), CU-19 (FA02), CU-21 (FA03).

---

### HU-03 · El que lleva mucho desconectado deja de ocupar plaza

**Qué queda sin cubrir.** La segunda fase de la inactividad. Un inactivo ocupa plaza, así que una
sala de cuatro con un inactivo **está llena para todo el mundo** y solo tiene tres para jugar.
`OB-50` decidió que, vencido un plazo, se le retira. Ningún caso de uso lo describe, y sin ello
una sala podría quedarse bloqueada indefinidamente por alguien que ya no va a volver.

**Por qué no es un caso de uso.** Lo dispara **un plazo que vence**. Nadie lo pide, ni el
afectado ni el anfitrión —el anfitrión tiene su propia vía, que sí es un caso de uso: `CU-21`
Expulsar—. La diferencia entre las dos es justamente el actor: la expulsión la decide alguien, la
retirada por inactividad no la decide nadie.

> **Como** jugador que quiere entrar a una sala o empezar una partida en ella,
> **quiero** que quien lleva desconectado más del plazo deje de ocupar su plaza,
> **para** que la sala no quede bloqueada por alguien que ya no está.

**Criterios de aceptación**

1. **Dado** un jugador marcado como inactivo, **cuando** se cumple el plazo de inactividad —tres
   minutos, valor configurable— **entonces** el sistema lo retira de la sala.
2. **Entonces** su color vuelve al conjunto de colores disponibles y su plaza queda libre, de
   modo que la sala vuelve a admitir a alguien.
3. **Entonces** los jugadores que permanecen dentro ven actualizarse la lista.
4. **Dado** que era el último de la sala, **entonces** la sala queda vacía y **se cierra**: su
   fecha de cierre queda escrita, su código queda libre y sus invitaciones pendientes se
   eliminan. No hace falta ninguna regla nueva para las salas que solo tenían inactivos.
5. **Dado** que el jugador está **dentro de una partida en curso**, **entonces** este plazo no se
   le aplica.
6. **Y** si vuelve a conectarse después de haber sido retirado, entra a la sala como cualquier
   otro jugador, por código o desde el listado si es pública: no tiene veto.

**Cubre:** `S-11`, `FC-43` · **Se apoya en:** `OB-50` (fase 2), `D-23`, y el valor de tres
minutos fijado en §5.4 de la base.

---

### HU-04 · Al terminar la partida, el resultado se calcula, se guarda y se ve

**Qué queda sin cubrir.** Es el hueco más grande del conjunto. Las descripciones dicen **cuándo**
termina una partida —tercera ronda jugada, quedar menos de dos, o nadie que vuelva tras una
caída— y con qué motivo se registra, pero **no dicen cómo se calcula el resultado ni qué se
escribe ni qué ve el jugador**:

- la puntuación final, que suma los castillos, el rey y **un punto por cada carta obtenida y no
  utilizada** `R 4.3` —lo único de la puntuación que solo ocurre al final—;
- el desempate: mayor número de castillos en los que puntuó y, si persiste, el puesto de mesa
  más bajo `DR-15`;
- la escritura de la puntuación y el **puesto final** en la participación de cada jugador con
  cuenta, y el borrado del estado de la partida;
- la pantalla de resultado, que el prototipo ya tiene como `PT-24` y a la que no llega ningún
  caso de uso.

**Por qué no es un caso de uso.** Nadie **pide** terminar la partida: termina sola, al agotarse
la tercera ronda o al quedarse sin jugadores suficientes. El jugador no ejecuta nada; recibe. Lo
que sí es caso de uso es **consultar** ese resultado después, y ya existe: `CU-10` y `CU-11`.
Convertir el fin de partida en caso de uso obligaría a ponerle como actor al jugador que casualmente
cerró el último turno, que no es quien persigue el objetivo.

> **Como** jugador que acaba de terminar una partida,
> **quiero** ver quién ganó y con cuántos puntos quedó cada uno, y que ese resultado quede
> guardado,
> **para** saber cómo terminó y poder consultarlo después en mi historial y verlo reflejado en el
> ranking.

**Criterios de aceptación**

1. **Dado** que se ha jugado el último turno de la tercera ronda, **cuando** ese turno se cierra,
   **entonces** el sistema calcula la puntuación final de cada jugador: los castillos en los que
   tiene **un único caballero propio**, contando superficie × nivel de la torre de ese caballero;
   los puntos del rey de cada ronda; y **un punto por cada carta obtenida y no utilizada**.
2. **Entonces** ordena a los jugadores por puntuación total; **en caso de empate**, por el número
   de castillos en los que puntuó; **si persiste**, por el puesto de mesa más bajo.
3. **Entonces** escribe en la participación de cada jugador **con cuenta** su puntuación final y
   su puesto final, y marca la partida como *terminada* con motivo *completada*, con su fecha de
   fin.
4. **Entonces** elimina el estado guardado de la partida, que solo existía para poder reanudarla.
5. **Entonces** muestra a todos los jugadores el resultado, con el puesto y los puntos de cada
   uno, **incluidos los invitados**, que se ven en la pantalla aunque no dejen ninguna fila.
5 bis. **Y** al cerrar el resultado devuelve a los jugadores **a la sala** en la que se jugó, que
   sigue abierta con su chat y su ranking ya actualizado; al menú principal solo si esa sala ya no
   existe.
6. **Dado** que la partida termina por quedar **menos de dos jugadores** —porque se retiraron o
   porque abandonaron—, **entonces** el motivo es *abandonada*, **gana el que queda** y se le
   registra el puesto 1.
7. **Dado** que **no volvió nadie** tras una caída, **entonces** el motivo es *interrumpida* y
   **no se registran puestos ni puntuaciones**; esa partida no cuenta para ningún ranking.
8. **Dado** que en la partida **no jugó ninguna cuenta**, **entonces** la partida se elimina al
   terminar junto con su estado, porque no podría consultarla nadie.
9. **Y** un jugador **retirado** por haber agotado su ventana de reconexión **no impide** nada de
   lo anterior: esa partida sigue contando para su ranking.

**Cubre:** `S-4`, `FC-30` · **Se apoya en:** `DR-02`, `R 4.1`, `R 4.2`, `R 4.3`, `DR-15`, `D-21`,
`D-25`, `P-07`, `PER §7.4` · **Alimenta a:** `CU-10`, `CU-11`, `CU-12` y `HU-05`.

**Resuelto:** al cerrar el resultado, **los jugadores vuelven a la sala en la que se jugó**,
porque la invitación es **a la sala** y en una sala pueden jugarse varias partidas `P-26`. Solo
vuelven al menú principal cuando esa sala ya no existe, que es el caso de una partida reanudada,
porque las salas no se reanudan. Esto obliga a cambiar el botón "Volver al menú" de `PT-24`.

---

### HU-05 · El ranking de la sala se actualiza al terminar cada partida

**Qué queda sin cubrir.** `CU-13` **muestra** el ranking de la sala al entrar, con sus puntos y
sus victorias, y su FA02 contempla que esté vacío. Pero **nada lo llena**: no hay ningún flujo
que lo actualice cuando una partida termina.

**Por qué no es un caso de uso.** Es una consecuencia automática de que una partida acabe, no
algo que alguien solicite. Consultarlo sí es interacción, y ya está descrito como parte de
`CU-13`. Nótese además que **este ranking no podría calcularse con una consulta**: incluye a los
invitados, de los que no se guarda ninguna fila, así que existe únicamente porque el sistema lo
acumula en memoria en este momento preciso.

> **Como** jugador que juega varias partidas seguidas en la misma sala,
> **quiero** que el ranking de la sala se actualice al terminar cada una,
> **para** ver cómo vamos entre nosotros, con invitados incluidos, sin tener que salir a mirar el
> ranking global.

**Criterios de aceptación**

1. **Dado** que una partida de la sala ha terminado con resultado, **cuando** el sistema registra
   ese resultado, **entonces** suma a cada participante sus puntos de esa partida en el ranking
   de la sala, y le suma una victoria si quedó primero.
2. **Entonces** el ranking incluye **también a los invitados**, identificados por su alias y su
   color.
3. **Entonces** los jugadores que estén en la ventana de la sala lo ven actualizado, ordenado por
   puntos.
4. **Dado** que la partida terminó como *interrumpida*, **entonces** no aporta nada al ranking,
   porque no tiene puestos ni puntuaciones.
5. **Y** el ranking **vive solo en la memoria del servidor**: muere con la sala y una caída del
   servidor se lo lleva entero.

**Cubre:** `S-7`, `FC-23` · **Se apoya en:** `D-29`, `P-26`, `FC-22` · **Afecta a:** `CU-13`
(paso 7 y FA02).

---

### HU-06 · La prohibición temporal se acaba sola

**Qué queda sin cubrir.** `CU-13` FA03 y `CU-19` FA03 **rechazan** al jugador con una sanción
vigente e indican hasta cuándo dura; `CU-23` FA04 la **aplica**. Nadie describe **el final**: que
al cumplirse la duración deja de aplicarse, sin que nadie la levante. Es imprescindible decirlo
porque `D-31` descartó el rol de administración: si el sistema no la caduca solo, **no la caduca
nadie**.

**Por qué no es un caso de uso.** El disparo es el reloj. El sancionado no hace nada para
recuperar el acceso, y no hay revisión humana ni apelación: las sanciones son automáticas por
umbral.

> **Como** jugador al que se le aplicó una prohibición temporal,
> **quiero** que deje de aplicarse en cuanto se cumple su duración,
> **para** poder volver a entrar a salas y a jugar sin que nadie tenga que levantármela.

**Criterios de aceptación**

1. **Dado** un jugador con una prohibición temporal vigente, **cuando** se alcanza su fecha de
   fin, **entonces** deja de impedirle entrar a salas y jugar partidas.
2. **Entonces** la fila de la sanción **no se borra**: se sigue contando para la escalera, porque
   el contador de prohibiciones no se reinicia nunca.
3. **Dado** que la sanción se cruzó **durante una partida**, **entonces** su duración empieza a
   contar **cuando entra en vigor** —al terminar esa partida—, no cuando se cruzó el umbral.
4. **Dado** que la sanción es **permanente**, **entonces** no caduca, y sigue impidiendo también
   iniciar sesión.
5. **Y** mientras la prohibición temporal está vigente, **el jugador sí puede iniciar sesión**:
   lo que no puede es entrar a salas ni jugar.

**Cubre:** `S-9`, y cierra el ciclo de `FC-39` y `FC-40` · **Se apoya en:** §4.7 de la base,
`OB-30`, `OB-37`, `OB-49`, `D-31` · **Afecta a:** `CU-01`, `CU-13`, `CU-19`, `CU-23`.

---

### HU-07 · El sistema deja rastro de lo que pasa, sin exponer a nadie

**Qué queda sin cubrir.** `RES-06` obliga a registrar los eventos **solo con log4net** y prohíbe
registrar contraseñas, tokens y datos personales. Las descripciones de casos de uso mencionan
errores de conexión en casi todas sus excepciones, pero **ninguna dice que queden registrados**,
y el registro no aparece en ninguna parte del conjunto.

**Por qué no es un caso de uso.** Es una **cualidad transversal**: atraviesa todas las
operaciones y ninguna la persigue. Quien recibe el valor no es un jugador, sino el equipo que
tiene que diagnosticar un fallo. No tiene ventana ni flujo, y modelarlo como caso de uso
obligaría a repetirlo dentro de los veintisiete.

> **Como** integrante del equipo de desarrollo,
> **quiero** que el sistema registre con log4net los errores y los eventos relevantes del
> servidor,
> **para** poder diagnosticar un fallo a partir del registro, sin que ese registro exponga datos
> de los jugadores.

**Criterios de aceptación**

1. **Dado** cualquier error que impida completar una operación —los que las descripciones
   recogen como excepciones—, **cuando** se produce, **entonces** queda registrado con su momento
   y su contexto.
2. **Entonces** el registro se hace **únicamente con log4net**: ninguna otra vía.
3. **Entonces** el registro **nunca** contiene contraseñas, hashes, códigos de recuperación,
   identificadores temporales de invitado ni datos personales.
4. **Y** el **único identificador de persona** admitido en el registro es el identificador
   numérico de la cuenta.

**Cubre:** `S-12` · **Se apoya en:** `RES-06`, `D-03`.

---

### HU-08 · Salir del juego

**Qué queda sin cubrir.** §4.2 de la base fija que «la opción **Salir** del menú principal cierra
la aplicación» `OB-15`. No tiene `FC-xx`, no es una de las doce situaciones `S-`, y ningún caso
de uso la menciona.

**Por qué no es un caso de uso.** No tiene postcondición dentro del sistema y **no habla con el
servidor**: la aplicación se cierra. Un caso de uso con un solo paso y sin efecto en el sistema no
aporta nada. Es la más pequeña de las ocho, y el equipo puede tratarla igualmente como un
requisito de interfaz en vez de como historia.

> **Como** jugador que ha terminado de jugar,
> **quiero** que la opción «Salir» del menú principal cierre la aplicación,
> **para** salir del juego sin tener que cerrar la ventana por fuera.

**Criterios de aceptación**

1. **Dado** un jugador en el menú principal, **cuando** selecciona «Salir», **entonces** la
   aplicación se cierra.
2. **Dado** que el jugador estaba identificado como **invitado**, **entonces** su identidad
   temporal desaparece con el cliente y no queda nada suyo en ninguna parte.
3. **Dado** que el jugador estaba **dentro de una sala**, **entonces** el servidor deja de recibir
   sus latidos y le aplica lo descrito en `HU-02`.

**Cubre:** §4.2 de la base, `OB-15`.

---

## 4. Lo que deliberadamente **no** se convierte en historia de usuario

Tan importante como cubrir lo que falta es no duplicar lo que ya está.

| Comportamiento | Por qué no lleva historia |
|---|---|
| Cierre del turno por tiempo, cierre de ronda, retiro por ventana agotada, reanudación, aplicación de la sanción | **Ya están descritos** dentro de un caso de uso (`S-2`, `S-3`, `S-5`, `S-6`, `S-8`). Una historia crearía una segunda fuente para la misma regla |
| El **latido** | Es el **mecanismo** que detecta la caída, no un comportamiento con valor propio. Vive dentro de `HU-02` y de `CU-26`, que es donde se nota |
| Asignación de color, generación del identificador del invitado, borrado del archivo del avatar, búsqueda por nombre de usuario | Son **pasos de una operación**, no objetivos de nadie. Ya están en el flujo de su caso de uso |
| Las acciones dentro del turno —colocar, mover, subir, construir, obtener y usar cartas— | Son **flujos alternos de `CU-25`** por decisión razonada: un solo objetivo, un solo actor, y un presupuesto de puntos que solo tiene sentido junto |
| Mantenimiento de los catálogos de parámetros | Son **de solo lectura y se cargan por script**. `D-31` descartó el rol de administración: no hay nadie que pueda pedir nada sobre ellos |
| Espectadores, notificaciones, logros, temporadas, bloqueo de jugadores, historial de invitaciones, cartas maestras | **Están fuera de alcance** en §4.11. Escribirles una historia sería ampliar el sistema por la puerta de atrás |

---

## 5. Cierre

### 5.1 Casos de uso que ya pueden redactarse completamente

**Veintidós de veintisiete**, y están redactados sin ninguna laguna en
`Documento-Casos-de-Uso-Torres.md` v3.0, que reúne **los veintisiete en un solo documento**,
`CU-01`, `CU-02` y `CU-03` incluidos y corregidos:

`CU-01` Iniciar sesión · `CU-02` Crear cuenta · `CU-03` Recuperar acceso · `CU-04` Jugar como
invitado · `CU-05` Cambiar el idioma · `CU-06` Modificar el perfil · `CU-08` Enviar una solicitud
de amistad · `CU-09` Responder una solicitud · `CU-10` Historial · `CU-11` Detalle de una
partida · `CU-12` Ranking global · `CU-13` Entrar a la sala · `CU-14` Unirse a una sala · `CU-15`
Crear una sala · `CU-17` Responder una invitación · `CU-18` Salir de la sala · `CU-19` Iniciar la
partida · `CU-21` Expulsar a un jugador · `CU-22` Enviar un mensaje al chat · `CU-23` Reportar a
un jugador · `CU-25` Jugar un turno · `CU-26` Reconectar a una partida en curso.

**Con las nueve aclaraciones del equipo integradas el 10 sep 2026, los veintiocho casos de uso
están completos**: los cinco que tenían un hueco lo tienen ya resuelto dentro de su flujo, y se
añadió `CU-28 Abandonar la partida`, que nace de la decisión de que los jugadores puedan
rendirse.

`CU-25` queda completo, pero **obliga a corregir la base**: `FC-29` y `S-3` sitúan el reparto de
construcciones en el cierre de ronda, y `R 1.3` y `R 2.2` lo sitúan **en cada turno**. La
descripción siguió el texto de las reglas.

### 5.2 Casos de uso con algún faltante

**Ninguno.** Los cinco que quedaban abiertos se cerraron con las aclaraciones del equipo:

| Caso de uso | Lo que le faltaba | Cómo quedó |
|---|---|---|
| `CU-07` Eliminar la cuenta | Qué pasa con la sala si elimina su cuenta estando dentro | **Sale de ella** aplicando las reglas de cualquier salida: libera su color, hereda el anfitrión el siguiente, y la sala se cierra si queda vacía |
| `CU-16` Invitar jugadores | Qué muestra si falla el correo | **El mismo criterio de `CU-03`**: informa, no reintenta, y la invitación queda registrada |
| `CU-20` Eliminar un amigo | Si pide confirmación | **Sí**, con las opciones "Eliminar" y "Cancelar" |
| `CU-24` Preparar la partida | Desconexión y tiempo de colocación | **El mismo tiempo y las mismas reglas que un turno**: 90 segundos y la misma ventana de reconexión |
| `CU-27` Volver a una partida reanudada | Cómo se le indica que hay partida esperándole | Se le **indica la caída en la interfaz**; si no salió, reanuda **sin intervención**; si salió, al volver se le indica que sigue en curso y elige **volver o abandonarla** |

**`CU-01`, `CU-02` y `CU-03` ya no están en esta lista.** Venían de la primera ronda con doce
defectos entre los tres —actor inexistente, precondiciones vacías, un flujo alterno citado y no
escrito, el baneo sin comprobar, un username «de 8 a 15» contra su propio mensaje «De 3 a 20», y
un enlace de recuperación que no tiene canje posible sin stack web—. Están corregidos y
reproducidos íntegros en el documento de casos de uso, con el detalle de cada cambio.

### 5.3 Historias de usuario necesarias

**Ocho**, ninguna con funcionalidad nueva:

| ID | Historia | Cubre |
|---|---|---|
| `HU-01` | Las salas no sobreviven al arranque del servidor | `S-1` |
| `HU-02` | El que pierde la conexión queda marcado, y la sala no se queda sin anfitrión | `S-10`, `FC-42` |
| `HU-03` | El que lleva mucho desconectado deja de ocupar plaza | `S-11`, `FC-43` |
| `HU-04` | Al terminar la partida, el resultado se calcula, se guarda y se ve | `S-4`, `FC-30` |
| `HU-05` | El ranking de la sala se actualiza al terminar cada partida | `S-7`, `FC-23` |
| `HU-06` | La prohibición temporal se acaba sola | `S-9` |
| `HU-07` | El sistema deja rastro de lo que pasa, sin exponer a nadie | `S-12` |
| `HU-08` | Salir del juego | §4.2 `OB-15` |

### 5.4 Información concreta que sigue pendiente de definir

**De los nueve huecos, ninguno.** El equipo los cerró el 10 sep 2026 y están integrados en las
descripciones; el detalle de cada uno está en la sección «Integración de las aclaraciones del
equipo» de `Documento-Casos-de-Uso-Torres.md`.

**Los demás documentos ya están actualizados.** `Documento-Base-Consolidada-Torres.md` recoge las
construcciones **por turno**, el **formato de la contraseña**, los **cinco minutos** del código,
`FC-44` y `CU-28`; el prototipo y su análisis tienen `PT-24` devolviendo **a la sala**, la opción
**"Rendirse"** en `PT-22`, el **reloj** de `PT-21`, los dos avisos nuevos en `PT-17` y la pantalla
nueva **`PT-27`**; y `code/database/` pasa de siete entidades a **diez**, con `password_recovery`,
`report` y `sanction`, los catálogos `sanction_level` y `system_parameter`, el código de sala de
cuatro caracteres y `invitation_token` retirado.

**Lo único que sigue sin valor fijado** son el **periodo del latido** y el **número de latidos
perdidos** para dar una conexión por caída: el proyecto declara el mecanismo, pero no los dos
números, así que `seed-configuration.sql` los deja sin sembrar en vez de inventarlos.

**Tres detalles se derivaron de reglas existentes** y están señalados por si el equipo quiere
corregirlos: que **rendirse tenga el mismo efecto que el retiro** por ventana agotada, que
**rendirse pida confirmación** como las demás acciones irreversibles, y que **agotar el tiempo de
una colocación** la cierre y pase al siguiente, como ocurre con un turno.

**Lo que no es un hueco de análisis, sino una decisión de proyecto:** la seguridad del
transporte. Con `SecurityMode.None` la credencial, el chat y el pase de asiento del invitado
**viajan sin cifrar**.
