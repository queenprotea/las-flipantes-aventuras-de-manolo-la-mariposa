# Contexto de temas — Juego de Torres

Archivo acumulador. Cada tema se registra en cuatro apartados fijos:

- **A · Concepto general** — se conserva tal como se recibió, aunque los ejemplos vengan de otro dominio.
- **B · Aplicación al dominio de Torres** — traslado explícito al proyecto, sin copiar el ejemplo ajeno.
- **C · Información del proyecto utilizada** — de dónde salió cada dato de la adaptación.
- **D · Criterios, decisiones y huecos** — lo acordado y lo que falta definir.

Los temas son **referencias independientes**: no se mezclan conceptos entre ellos.

Documentos base: `Analisis-Stakeholders-Concerns-Torres.md`, `Analisis-Arquitectonico-Torres.md`, `Analisis-Persistencia-Torres.md`, `code/database/`.

---

# Tema 01 · Árbol de utilidad y requisitos arquitectónicamente significativos

*Origen del material:* diapositivas DDS · S07 · LIS · FEI-UV. Fuentes citadas en ellas: SAIP 4.ª ed., cap. 19; Cervantes y Kazman, cap. 2.
*Estado:* concepto registrado. Aplicación **estructural** hecha; hojas (escenarios) **pendientes** — las proporcionará el usuario.

## A · Concepto general

### A.1 Qué es

El árbol de utilidad organiza los requisitos de calidad de un sistema en una estructura de **cuatro niveles**, de lo abstracto a lo comprobable:

```
Utilidad  →  Categoría  →  Refinamiento  →  Hoja
 (raíz)      (atributo     (¿de qué,        (escenario de seis
              de calidad)   exactamente?)    partes, anotado)
```

- **Utilidad (raíz).** Nodo artificial. Existe para poder mirar todo el sistema desde un solo lugar.
- **Categoría.** El atributo de calidad (disponibilidad, modificabilidad, desempeño, seguridad, integrabilidad…).
- **Refinamiento.** Acota la categoría a un asunto concreto. Es *el problema*, nunca la solución.
- **Hoja.** **Siempre** un escenario de seis partes; nunca un adjetivo.

### A.2 Formato de anotación de la hoja

| Campo | Qué responde |
|---|---|
| Categoría | ¿Qué propiedad? |
| Refinamiento | ¿De qué, exactamente? |
| Hoja | Escenario de seis partes |
| Procedencia | ¿De qué CON- salió? |
| Par | Una razón por cada letra |

**Par = (Importancia, Dificultad)**, en escala A / M / B:
- **Importancia** — qué se pierde y **quién** lo pierde. Es del negocio.
- **Dificultad** — qué tanto cuesta lograrlo. Es del equipo técnico.

### A.3 Las seis partes del escenario

Fuente · Estímulo · Artefacto · Ambiente · Respuesta · **Medida de respuesta**. El escenario debe ser **refutable** (medible) y llevar **procedencia**.

### A.4 De dónde salen los requisitos de calidad

| Procedencia | Qué se obtiene ahí |
|---|---|
| Del documento de requisitos | Pocos, y rara vez los que deciden. Suelen ser adjetivos sueltos en un apartado al final |
| De los interesados, preguntándoles | Los que nadie escribió porque «se dan por hecho». Es lo que produce la entrevista |
| De los objetivos del negocio | Los que no son de nadie en particular y condicionan todo: costo, plazo, obligación legal, reputación |

### A.5 Importante ≠ arquitectónicamente significativo

Prueba central, dos preguntas:

1. **¿Qué estructura del sistema cambiaría si quitáramos este requisito?**
2. ¿Y si la medida fuera notablemente más laxa?

Si la respuesta a la primera es «ninguna», el requisito es **legítimo pero no es un driver**: se atiende, no se diseña alrededor de él.

**Tres marcas de un requisito significativo:**

| Marca | Pregunta que hay que poder contestar |
|---|---|
| Efecto profundo | ¿Obliga a estructurar el sistema de otra manera, o se resuelve escribiendo más código dentro de lo que ya existe? |
| Valor alto | ¿Alguien pierde dinero, usuarios, licencia o reputación si no se cumple? |
| Alcance amplio | ¿Toca varios elementos del sistema, o se queda dentro de uno solo? |

Corolario del concepto: **un requisito funcional puede ser significativo, y uno de calidad puede no serlo.**

### A.6 Cinco maneras de arruinar un árbol

1. Copiar completas las categorías de un catálogo de calidad y dejar la mitad sin hojas debajo.
2. Escribir refinamientos que ya son soluciones: «caché», «réplica», «microservicios».
3. Poner como hoja un adjetivo, y no un escenario de seis partes.
4. Marcar todo **(A, A)**: la manera elegante de no decidir.
5. Confundir **dificultad** con **desconocimiento**: «no sabemos hacerlo» y «el problema es duro» se resuelven distinto.

### A.7 Criterios de calibración del par (taller)

- Una línea de justificación **por cada letra**: por qué esa importancia, por qué esa dificultad.
- **A lo sumo dos** escenarios pueden quedar en (A, A).
- Si un escenario parece difícil, declarar si es **problema duro** o **falta de conocimiento del equipo**.
- Un renglón sin hoja vale tanto como los llenos: señala un hueco de análisis, no una ausencia de importancia.

> Nota de dominio: los ejemplos de la clase (expediente clínico, E1–E6, «procede de CON-07» de ese caso) pertenecen a **otro dominio** y no se trasladan a Torres. Se conservan aquí solo como ilustración del formato.

## B · Aplicación al dominio de Torres

### B.1 La raíz

`Utilidad del juego de Torres`. Único punto desde el que se mira el sistema completo: partida en curso, persistencia, cuentas y salas, operación del servidor.

### B.2 Nivel de categorías

Las categorías **no se copian de un catálogo** (riesgo 1): salen de los concerns ya establecidos y clasificados en `Analisis-Stakeholders-Concerns-Torres.md` §4.2.

| Categoría candidata | Concern que la sostiene | Estado de la clasificación en el proyecto |
|---|---|---|
| Disponibilidad | CON-01 · Continuidad ante una desconexión | Confirmado |
| Modificabilidad | CON-02 · Ajuste de reglas y parámetros | Confirmado |
| Corrección funcional | CON-03 · Correspondencia entre lo jugado y lo puntuado | Confirmado |
| Corrección funcional bajo concurrencia | CON-05 · Aislamiento entre partidas simultáneas | **Parcial** — falta la cifra objetivo; no es escalabilidad |
| Seguridad — confidencialidad | CON-06 · Qué información personal se guarda y registra | Confirmado, alcance limitado al registro de eventos |
| Testabilidad | CON-08 · Comprobar que una regla hace lo que dice | Confirmado |
| Analizabilidad (diagnóstico) | CON-09 · Reconstruir qué ocurrió cuando algo falla | Confirmado |

**CON-04** (conservación del resultado) y **CON-07** (jugar con cuenta o como invitado) están declarados **sin atributo de calidad**: son funcionales. Por A.5 pueden aun así someterse a la prueba de significancia, pero **no se les fabrica una categoría** para meterlos en el árbol.

*Nota:* el proyecto **no ha fijado un catálogo de atributos de calidad** (cuestión abierta 18). El árbol se construye sobre los concerns, no sobre el catálogo.

### B.3 Nivel de refinamiento

**No está definido todavía.** No se inventa. Cuando se defina, cada refinamiento debe anclarse en material ya establecido del proyecto, y **enunciar el problema, no la solución**. Anclajes disponibles hoy:

| Categoría | Material del proyecto donde debe apoyarse el refinamiento |
|---|---|
| Disponibilidad | DEP-06 (ventana de reconexión ← estado del turno), DEP-07 (continuidad ← permanencia de jugadores), TEN-03 `[BLOQUEANTE]`, TEN-04, `Persistencia` §6 (reconexión tras caída), P-29, P-38 |
| Modificabilidad | CON-02, TEN-01 y TEN-02 `[BLOQUEANTE]`, TEN-06, DEP-08 (ubicación de la configuración), Anexo A (P-11, parámetros de reglas), `seed-configuration.sql` |
| Corrección funcional | DEP-02, DEP-03, DEP-04, DEP-05, TEN-05 (sorteo del empate), P-09 (empates) |
| Concurrencia | CON-05, `Persistencia` §4.2 (estado en memoria del servidor) |
| Seguridad — confidencialidad | CON-06, `Persistencia` §3.1 `Cuenta`, Anexo B (P-12, rol de administración) |
| Testabilidad | CON-08, TEN-05, TEN-06 |
| Analizabilidad | CON-09, registro de eventos |

**Riesgo 2 aplicado a Torres:** «configuración externa» y «motor de reglas» aparecen en el análisis como *soluciones prescritas, no decididas*. **No pueden ser refinamientos.** El refinamiento sería el asunto que las motiva (p. ej. qué clase de cambio de regla debe absorberse sin recompilar), no la técnica.

### B.4 Nivel de hoja

Pendiente. Las proporcionará el usuario. Al incorporarlas, cada una debe traer:

- las **seis partes**, con **medida de respuesta** refutable;
- **procedencia** con identificador **del proyecto**;
- el **par (Importancia, Dificultad)** con una razón por letra.

### B.5 Procedencia — mapa al proyecto

| Procedencia del concepto (A.4) | Su equivalente real en Torres |
|---|---|
| Documento de requisitos | `Reglas_del_Juego_de_Torres.docx` (v3.0, **borrador**) y `Estandar-de-codificacion-v7.docx`. Ojo con OBS-01: el estándar usa ejemplos de otro juego |
| Interesados, preguntándoles | STK-01 Jugador, STK-02 Equipo de desarrollo, y demás fichas STK. Material ya recogido: respuestas **P-xx** integradas en `Analisis-Persistencia-Torres.md` |
| Objetivos del negocio | **No definidos explícitamente en el proyecto.** Ver hueco H-3 |

Identificadores válidos como procedencia: `CON-`, `STK-`, `DEP-`, `TEN-`, `P-`, `OBS-`, más la referencia al documento de reglas. **`CON-07` del ejemplo clínico de la clase no tiene relación con el CON-07 de Torres** (jugar con cuenta o como invitado).

### B.6 El par en Torres

- **Importancia** — «qué se pierde y quién lo pierde» se responde nombrando un **STK** concreto y lo que deja de obtener.
- **Dificultad** — «qué tanto cuesta» se responde contra la arquitectura del proyecto, no contra la sensación del equipo.
- **Riesgo 5 aplicado a Torres:** TEN-07 (construir sobre reglas en estado de borrador) es fuente de *desconocimiento*, no de dificultad intrínseca. Las tres tensiones `[BLOQUEANTE]` (TEN-01, TEN-02, TEN-03) también: mientras no se decidan, un escenario puede parecer «difícil» cuando en realidad está **indefinido**. Eso debe declararse, no puntuarse.

## C · Información del proyecto utilizada

- `Analisis-Stakeholders-Concerns-Torres.md`: §4.1 refinamiento de frases recibidas, **§4.2 tabla concern → atributo de calidad**, §5 relaciones entre concerns, §10 cuestiones abiertas (18: sin catálogo de referencia).
- `Analisis-Arquitectonico-Torres.md`: §2 stakeholders, §4 DEP-01…DEP-08, §5 TEN-01…TEN-07 con marcas `[BLOQUEANTE]`, OBS-01.
- `Analisis-Persistencia-Torres.md`: §4.2 estado en memoria, §6 reconexión, Anexo A (P-11), Anexo B (P-12), P-09, P-29, P-38.
- `code/database/seed-configuration.sql` como evidencia de que hay parámetros configurables.

## D · Criterios, decisiones y huecos

### Criterios adoptados para este tema

- **C-01.** Las categorías del árbol de Torres salen de CON-01…CON-09, no de un catálogo externo.
- **C-02.** Un concern declarado «sin atributo» (CON-04, CON-07) no genera categoría.
- **C-03.** Ningún refinamiento puede ser una solución técnica.
- **C-04.** Toda hoja lleva procedencia con identificador del proyecto.
- **C-05.** Máximo dos hojas en (A, A) por ejercicio de calibración.
- **C-06.** Si un escenario es difícil por indefinición (tensión bloqueante o regla en borrador), se declara como desconocimiento, no como dificultad.

### Huecos — información del proyecto que falta para completar el árbol

- **H-1.** **Refinamientos no definidos** para ninguna categoría.
- **H-2.** **Medidas de respuesta sin valor.** CON-05 no tiene cifra objetivo; la ventana de reconexión sigue abierta (TEN-03 `[BLOQUEANTE]`, P-29). Sin número no hay hoja refutable.
- **H-3.** **Objetivos de negocio no enunciados.** Sin ellos, la letra de **Importancia** no tiene contra qué justificarse; hoy solo puede apoyarse en las fichas STK.
- **H-4.** ~~Numeración de stakeholders inconsistente entre documentos.~~ **RESUELTO (4 sep 2026)** por `Base-Oficial-STK-CON-DEP-Torres.md`: rigen tres stakeholders (STK-01 Jugador, STK-02 Equipo, STK-03 Receptor de la entrega). El reanálisis encontró además que `CON-01` y `CON-02` estaban **invertidos** entre los dos análisis. **Toda procedencia se cita ahora contra la base oficial, no contra los análisis previos.**
- **H-5.** **Catálogo de atributos de calidad sin fijar** (cuestión abierta 18): los nombres de categoría son los habituales, pero no hay norma de referencia adoptada. Registrada como **Q-22** en la base oficial.

> **Nota de vigencia (4 sep 2026).** La tabla de categorías de B.2 sigue siendo válida, pero su fuente autorizada pasa a ser §3.1 de `Base-Oficial-STK-CON-DEP-Torres.md`, donde CON-01 gana la variante *caída del servidor* y CON-06 amplía su alcance a la persistencia. H-1, H-2 y H-3 siguen abiertos.

---

# Tema 02 · El escenario de calidad: seis partes, general y concreto

*Origen del material:* diapositivas DDS · S06 · LIS · FEI-UV (láminas 2, 5, 6, 10, 16, 23, 24, 25 de 31). Fuentes citadas en ellas: SAIP 4.ª ed., §3.3 y figura 3.1; §4.1, tabla 4.2 y figura 4.1.
*Relación con el Tema 01:* el árbol de utilidad **usa** el escenario como hoja, pero son temas distintos y se conservan por separado. Tema 02 trata cómo se escribe y se refuta **un** escenario; Tema 01, cómo se organizan y priorizan muchos.
*Estado:* concepto registrado. Aplicación a Torres hecha sobre los dos escenarios que el equipo ya redactó; el texto literal de esos escenarios **no está en el repositorio** (hueco H-6).

## A · Concepto general

### A.1 Un escenario es un párrafo hecho de piezas

Punto de partida de la clase: un párrafo en prosa corriente ya contiene todas las piezas. Lo que se pide identificar en él, sin nombrarlas técnicamente todavía:

- qué ocurre · quién lo provoca · sobre qué parte cae
- en qué condición está · qué tiene que hacer · cómo comprobaríamos que lo hizo

### A.2 Las seis partes, con nombre

```
Fuente → Estímulo →  Artefacto  → Respuesta → Medida de la respuesta
                     Ambiente
```

| Fragmento del párrafo de ejemplo | Se llama |
|---|---|
| «El servicio institucional de identidad…» | Fuente del estímulo |
| «…deja de responder» | Estímulo |
| «…la consulta de antecedentes ya cargados en la sesión abierta» | Artefacto |
| «…en operación normal» | Ambiente |
| «…le permite consultar… restablece… deja registrado» | Respuesta |
| «…en menos de cinco segundos» | Medida de la respuesta |

*Ejemplo de dominio clínico; se conserva solo como ilustración del mapeo.*

### A.3 Nadie escribe un escenario desde cero: general → concreto

El **escenario general** de una propiedad es un **menú**: la lista de valores posibles de cada parte para ese atributo. El **escenario concreto** se deriva eligiendo del menú, pertenece a *este* sistema, y **es lo que se entrega**.

Escenario general de **disponibilidad** (SAIP 4.ª ed., §4.1, tabla 4.2):

| Parte | Menú |
|---|---|
| Fuente | Interna o externa: personas, hardware, software, infraestructura física |
| Estímulo | Una falla: omisión, caída, tiempo incorrecto, respuesta incorrecta |
| Artefacto | Procesadores, canales de comunicación, almacenamiento, procesos |
| Ambiente | Operación normal, arranque, apagado, modo de reparación, operación degradada, sobrecarga |
| Respuesta | Evitar que la falla se vuelva una caída · detectarla · registrarla · notificar · recuperarse · operar en modo degradado |
| Medida | Intervalo en que debe estar disponible · porcentaje · tiempo hasta detectar · tiempo hasta reparar · tiempo admisible en modo degradado |

### A.4 La misma forma, otra medida

El formato de seis partes **no cambia** de un atributo a otro; lo que cambia es la **medida**. En un escenario de modificabilidad la medida se expresa en **coste de cambio** —cuántos elementos se tocan, cuáles no deben tocarse, en cuánto tiempo queda desplegado—, no en tiempo de respuesta. Frase de la lámina: *«Misma forma, misma precisión, medida de la otra categoría. Ni un cronómetro aparece aquí.»*

### A.5 Las semillas C1 a C4

Las condiciones que un proyecto ya declaró en su propuesta **son ya la lista de sus escenarios**:

| Condición del proyecto | Qué escenarios produce |
|---|---|
| **C1** · Estado compartido con alguien que manda sobre él | Integridad del estado: qué pasa cuando dos versiones no coinciden |
| **C2** · Concurrencia real | Estímulos simultáneos: dos acciones que llegan casi a la vez |
| **C3** · Una falla observable | Estímulos de falla: qué se cae, y qué debe seguir funcionando |
| **C4** · Una regla que puede cambiar | Modificabilidad: qué se toca cuando esa regla cambia |

### A.6 Las preguntas que se le hacen a un escenario

Cuando un equipo lee su escenario, el grupo pregunta **solo** esto:

1. ¿Qué se mide exactamente, y **desde qué instante** se cuenta?
2. ¿En qué **ventana**, y con qué **frecuencia** tiene que cumplirse?
3. ¿**Quién lo nota** si no se cumple?
4. ¿De qué **CON-** salió, y de quién era ese interés?

Y la de siempre: **¿podría alguien demostrarnos mañana que ayer no se cumpli[ó]?** *(el texto de la captura está cortado en esta última palabra).*

### A.7 Refutación cruzada

Ejercicio: se intercambian tres escenarios con otra pareja, cuatro minutos por sentido. Una sola consigna:

> **Encuentren una situación en la que el escenario se cumpla al pie de la letra y el sistema siga siendo inservible.**

Reglas del ejercicio: **no proponer solución**, **no reescribir el escenario ajeno**; se entrega la situación y ya. Después, cuatro minutos para corregir los propios.

*Ejemplo de refutación contractual (dominio ajeno, solo ilustrativo):* un SLA promete 99.99 % de disponibilidad mensual pero excluye del cálculo ciertos periodos de mantenimiento programado. El sistema queda fuera de servicio seis horas el lunes de mayor consulta, todas dentro de una exclusión válida. El indicador contractual se cumple y aun así el servicio resulta inaceptable.

### A.8 Antipatrones: si te descubres escribiendo esto, pregúntate lo otro

| Lo que ibas a escribir | Lo que conviene preguntarte |
|---|---|
| «Fuente: el usuario. Estímulo: el usuario usa el sistema» | ¿Es un evento o es la operación normal? |
| «Ambiente: producción» | ¿En qué condición? Normal, degradada, en despliegue, con una dependencia caída |
| «Artefacto: el sistema» | ¿Qué elemento concreto tiene que responder? |
| «Respuesta: el sistema responde correctamente» | Correcto según qué. **Nombra la obligación** |
| «Medida: 99.9 %» | ¿Qué se mide, en qué ventana, quién lo nota? ¿Cuentan las caídas programadas? |
| «El equipo tarda poco en cambiarlo» | ¿Poco comparado con qué? ¿Cuántos elementos se tocan? |
| «Debe ser rápido, en menos de dos segundos» | Le pegaste un número a un adjetivo. Falta todo lo demás |
| Ocho escenarios de disponibilidad | ¿Y las otras propiedades de tu mapa? |
| Un escenario sin `CON-` de origen | ¿De qué preocupación, y de quién, salió esto? |

## B · Aplicación al dominio de Torres

### B.1 Los dos escenarios que el equipo ya redactó

Torres tiene ya **un escenario de Modificabilidad y uno de Disponibilidad**, aportados por el equipo (no derivados de las reglas ni del estándar). El análisis existente les levantó objeciones que son, punto por punto, las de A.6 y A.8:

| Objeción ya registrada | Antipatrón / pregunta del Tema 02 que la explica |
|---|---|
| **Q-15** · el artefacto es el «manejador de partidas», que no aparece ni en las reglas ni en el estándar, y no puede pertenecer a dos capas a la vez | A.8 · «Artefacto: el sistema» → ¿qué elemento **concreto** responde? Aquí el elemento **no existe todavía** |
| **Q-16** · la medida de Modificabilidad describe que la configuración se aplique al iniciar la partida — eso es comportamiento funcional, no coste de cambio | **A.4** · misma forma, otra medida. Una medida de modificabilidad se expresa en artefactos tocados, recompilación/redespliegue o tiempo |
| **Q-17** · el ambiente está fijado en «servidor en estado normal», lo que excluye por construcción el fallo del servidor | A.8 · «Ambiente: producción» → ¿en qué condición? La condición elegida **borra** la falla que importa |
| Recomendación registrada: la medida de Disponibilidad dice «mientras la partida continúe activa», más laxa que la regla y sin plazo | A.6 · ¿en qué ventana, desde qué instante? Los valores de `R 5.2` (tiempo restante del turno, o 90 s) sí son verificables |

**Refutación cruzada aplicada (A.7).** Dos refutaciones ya están implícitas en el análisis y no requieren datos nuevos:

- *Escenario de Disponibilidad:* se cumple al pie de la letra —el jugador se reconecta mientras la partida sigue activa y el servidor está normal— y el sistema es inservible si **el servidor se cae**: el ambiente elegido excluye ese caso por construcción (Q-17).
- *Escenario de Modificabilidad:* se cumple al pie de la letra —la configuración se aplica al iniciar la partida— y sigue siendo inservible si para cambiar un valor hay que **tocar `Game.Domain` y recompilar** (Q-16).

Ambas se entregan como *situación*, sin proponer solución, conforme a la regla del ejercicio.

### B.2 El escenario general de disponibilidad, instanciado a Torres

Menú de A.3 con los valores que **el proyecto ya respalda**. Lo que no tiene respaldo se deja marcado, no se rellena.

| Parte | Valores con fuente en el proyecto | Sin definir |
|---|---|---|
| Fuente | Jugador y su conexión (CON-01); servidor e infraestructura (STK-05, operador) | — |
| Estímulo | Desconexión de un jugador durante su turno (CON-01, `R 5.2`); caída del servidor (`Persistencia` §6, Q-17) | Otras clases de falla (respuesta incorrecta, tiempo incorrecto) no están tratadas |
| Artefacto | Estado de la partida en curso (`Persistencia` §4.1); sala (§3.3); participación (§3.6) | El «manejador de partidas» del escenario del equipo **no existe** como elemento definido (Q-15) |
| Ambiente | Partida en curso; arranque del servidor (`Persistencia` §3.3 `[PROPUESTA]`: al arrancar se cierran las salas que quedaran); reanudación tras caída (§6.2) | «Modo degradado» **no está definido** para Torres |
| Respuesta | Conservar el estado y reanudar (§6.1–6.2); permitir el retorno del invitado (P-38, vale de asiento); aplicar el retiro del jugador (`R 5.2`) | Detección, registro y notificación no están especificados (enlaza con CON-09) |
| Medida | Tiempo restante del turno, o 90 s (`R 5.2`); los 90 s **no corren** mientras el servidor está caído (P-29 `[PROPUESTA]`) | Porcentaje de disponibilidad, tiempo hasta detectar y tiempo hasta reparar: **no hay cifras** |

### B.3 Las semillas C1–C4 en Torres

⚠ **Choque de nombres.** El proyecto ya usa `CON-C1`…`CON-C5` para *candidatos a concern evaluados y no adoptados* en `Analisis-Arquitectonico-Torres.md`. **No son las semillas C1–C4 de esta clase.** Para evitar confusión, aquí se escriben **S06-C1…S06-C4**.

| Semilla | ¿Se cumple la condición en Torres? | Material que produce el escenario |
|---|---|---|
| **S06-C1** · Estado compartido con quien manda sobre él | **Sí.** El estándar decide que el cliente «no decide el resultado de ninguna acción del juego» `[ESTÁNDAR §2.1]`; hay mazo, información oculta, puntuación parcial que decide quién coloca el rey (DEP-05) y sorteo del empate (TEN-05, `R 2.3`) | Escenarios de integridad del estado. Ojo: `CON-C2` está **no adoptado** por falta de evidencia de riesgo de manipulación |
| **S06-C2** · Concurrencia real | **Parcial.** Entre partidas: CON-05, aislamiento. Dentro de una partida el turno es secuencial, pero los dos plazos duros de 90 s crean una carrera real entre la acción del jugador y el vencimiento (`R 5.1`, `R 5.2`, `CON-C3`, TEN-04) | Escenarios de estímulos simultáneos. **Falta la cifra** de partidas concurrentes (Q-19) |
| **S06-C3** · Una falla observable | **Sí.** Desconexión del jugador (CON-01) y caída del servidor (Q-17, `Persistencia` §6) | Escenarios de falla. Es donde vive el escenario de Disponibilidad ya redactado |
| **S06-C4** · Una regla que puede cambiar | **Sí.** CON-02; tablas por número de jugadores `R 1.3`, costes `R 3.2`, cartas maestras previstas (TEN-02), reglas en v3.0 **borrador** (TEN-07) | Escenarios de modificabilidad. La recomendación registrada ya dice qué es configurable: **los parámetros, no la geometría** — `R 1.2` declara fija la distribución |

### B.4 Los antipatrones, en la lengua de Torres

- «Artefacto: el sistema» → nombrar un elemento del estándar: `Game.Domain`, `Game.Services`, `Game.Persistence`, o una entidad de `Persistencia` §3. **No** un elemento inventado (Q-15).
- «Ambiente: producción» → en Torres las condiciones reales son: partida en curso · turno del jugador X · arranque del servidor · reanudación tras caída · sala abierta sin partida. Elegir una y declararla.
- «Respuesta: el sistema responde correctamente» → nombrar la obligación citando la regla: `R 5.2`, `R 1.3`, `R 3.2`.
- «Medida: 99.9 %» → Torres **no tiene** ningún porcentaje acordado; usar los plazos de `R 5.2`, que sí existen.
- «Un escenario sin `CON-` de origen» → toda hoja debe citar CON-01…CON-09.
- «Ocho escenarios de disponibilidad, ¿y las otras propiedades?» → hoy Torres tiene **dos** escenarios y **nueve** concerns. El desequilibrio es el contrario: faltan propiedades, no sobran escenarios de una.

## C · Información del proyecto utilizada

- `Analisis-Arquitectonico-Torres.md`: §3 *Candidatos a concern* (CON-C1…CON-C5), **Q-15, Q-16, Q-17, Q-19**, nota final que declara que los escenarios de Modificabilidad y Disponibilidad fueron aportados por el equipo.
- `Analisis-Stakeholders-Concerns-Torres.md`: §12 *Sobre los escenarios ya redactados* (medida de Modificabilidad → coste de cambio; medida de Disponibilidad → plazos de `R 5.2`), cuestión 17 (parámetros, no geometría).
- `Analisis-Persistencia-Torres.md`: §3.3 `Sala`, §3.6 `Participación`, §4.1 estado de la partida, §6.1–6.5 reconexión, P-29, P-38.
- Reglas citadas de segunda mano a través de esos análisis: `R 1.2`, `R 1.3`, `R 2.3`, `R 3.2`, `R 5.1`, `R 5.2`. Estándar: `§2.1`, `§2.2`.

## D · Criterios, decisiones y huecos

### Criterios adoptados para este tema

- **C-07.** Todo escenario de Torres se escribe derivándolo del escenario **general** de su propiedad, no desde cero.
- **C-08.** El artefacto debe ser un elemento que **exista** en el estándar o en el modelo de persistencia. Si no existe, el escenario no está listo.
- **C-09.** El ambiente debe nombrar una condición concreta de Torres; «en operación normal» solo vale si la exclusión de la falla es **deliberada y declarada**.
- **C-10.** La medida de un escenario de modificabilidad se expresa en **coste de cambio**, nunca en tiempo de respuesta.
- **C-11.** Antes de dar por bueno un escenario se le aplica la refutación de A.7 y las cuatro preguntas de A.6.
- **C-12.** Las semillas de la clase se citan como **S06-C1…S06-C4** para no colisionar con `CON-C1…CON-C5` del proyecto.

### Huecos

- **H-6.** **El texto literal de los dos escenarios del equipo no está en el repositorio.** Los análisis los critican, pero no los transcriben. Para refutarlos formalmente hace falta su redacción exacta.
- **H-7.** Q-16 sigue abierta: no se ha elegido si la Modificabilidad se mide en coste de cambio o en comportamiento de arranque, ni se ha fijado umbral.
- **H-8.** Q-17 sigue abierta: no se ha declarado si la Disponibilidad cubre la caída del servidor.
- **H-9.** Q-15 sigue abierta: el artefacto «manejador de partidas» no tiene capa asignada.
- **H-10.** No existe definición de **modo degradado** para Torres, aunque el menú general de disponibilidad lo ofrece como ambiente y como respuesta.
