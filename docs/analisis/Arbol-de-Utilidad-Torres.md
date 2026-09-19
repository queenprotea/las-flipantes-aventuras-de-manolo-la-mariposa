# Árbol de utilidad — Juego de Torres

**Versión 1.0 · 4 sep 2026.**

Construido sobre `Base-Oficial-STK-CON-DEP-Torres.md` v4.1 y `Escenarios-Arquitectonicos-Torres.md` v1.0.

**Estructura de cuatro niveles:** `Utilidad → Categoría → Refinamiento → Hoja`. La raíz existe para mirar todo el sistema desde un solo lugar. **La hoja es siempre un escenario de seis partes, nunca un adjetivo.**

**Las categorías no se copian de un catálogo.** Salen de los concerns de §2 de la base. `ISO/IEC 25010` `DR-16` se usa para *nombrar* el atributo, no para decidir cuáles hay.

**Los renglones sin hoja son parte del árbol**, no un error: muestran lo que todavía nadie preguntó.

---

## 1. El árbol

```
Utilidad del juego de Torres
│
├── Disponibilidad ─────────── CON-01
│   ├── Continuidad del jugador ante la pérdida de su conexión
│   │   └── D-1 · Cae en su propio turno con T segundos restantes ......... (A, M)
│   └── Continuidad de la partida ante la caída del servidor
│       └── (sin hoja) ................................................... —
│
├── Modificabilidad ────────── CON-02
│   ├── Coste de aplicar un cambio en un valor que las reglas declaran variable
│   │   └── M-1 · Cambia el coste en PA de una acción ..................... (A, B)
│   └── Coste de un cambio de reglas que alcanza al dominio
│       └── (sin hoja) ................................................... —
│
├── Corrección funcional ───── CON-03 · CON-05
│   ├── Correspondencia entre el estado del tablero y la puntuación asignada
│   │   └── F-1 · Cierre de puntuación de una ronda con un jugador retirado  (A, A)
│   └── Independencia del resultado de una partida respecto de las demás
│       └── (sin hoja) ................................................... —
│
├── Testabilidad ───────────── CON-08
│   └── Coste de ejercitar una regla del dominio
│       └── T-1 · Probar una regla sin levantar nada ...................... (A, A)
│
├── Analizabilidad ─────────── CON-09
│   └── Diagnóstico de un fallo que ya no puede reproducirse
│       └── L-1 · Una operación falla y la partida ya no existe ........... (M, B)
│
└── Seguridad ──────────────── CON-06 · CON-10
    ├── Qué información personal queda expuesta, y dónde
    │   └── (sin hoja) ................................................... —
    └── Que solo el titular pueda actuar como su cuenta
        └── (sin hoja) ................................................... —
```

**Dos hojas en (A, A): F-1 y T-1. Ahí empieza el diseño.**

---

## 2. Tabla del árbol

| Categoría | Refinamiento | Hoja | Procedencia | Par |
|---|---|---|---|---|
| Disponibilidad | Continuidad del jugador ante la pérdida de su conexión | **D-1** | CON-01 · STK-01 | **(A, M)** |
| Disponibilidad | Continuidad de la partida ante la caída del servidor | — | CON-01 V2 | — |
| Modificabilidad | Coste de aplicar un cambio en un valor declarado variable | **M-1** | CON-02 · STK-02 | **(A, B)** |
| Modificabilidad | Coste de un cambio de reglas que alcanza al dominio | — | CON-02 variante C | — |
| Corrección funcional | Correspondencia entre estado del tablero y puntuación | **F-1** | CON-03 · STK-01 | **(A, A)** |
| Corrección funcional | Independencia del resultado entre partidas simultáneas | — | CON-05 | — |
| Testabilidad | Coste de ejercitar una regla del dominio | **T-1** | CON-08 · STK-02 | **(A, A)** |
| Analizabilidad | Diagnóstico de un fallo que ya no puede reproducirse | **L-1** | CON-09 · STK-02 | **(M, B)** |
| Seguridad — confidencialidad | Qué información personal queda expuesta, y dónde | — | CON-06 | — |
| Seguridad — autenticidad | Que solo el titular pueda actuar como su cuenta | — | CON-10 | — |

**Fuera del árbol, a propósito:** **CON-04** (conservación del resultado) y **CON-07** (cuenta o invitado) están declarados **sin atributo de calidad**. Son funcionales. Meterlos exigiría inventarles una exigencia que nadie ha enunciado.

---

## 3. Justificación del par — una línea por letra

**Contra qué se justifica cada letra.** La **Importancia** debería medirse contra objetivos de negocio; Torres solo tiene dos, `OBJ-01` —se evalúa el código, no el acabado visual— y `OBJ-02` —las dos personas trabajan de forma autónoma—, y **ninguno habla del jugador**. Por eso, donde la importancia recae en el jugador se justifica contra **requisito de fase (`N1`/`N2`) o regla escrita (`R`)**, no contra un objetivo. Queda declarado, no disimulado. La **Dificultad** se justifica contra `ORG-01`…`ORG-06` y las tensiones abiertas.

### D-1 · Disponibilidad — (A, M)

**A (Importancia).** La reconexión es **requisito de fase** `N2.12` y las reglas le dedican `R 5.2` entera con plazos y consecuencia; quien no se cumple pierde el turno o queda **retirado**, y con 2 jugadores su retiro termina la partida para el otro también.

**M (Dificultad).** El mecanismo existe y es estándar —canal duplex de CoreWCF `DEP-E11`— y `DR-20` evita tener que pausar el reloj; pero el componente de reconexión **no puede ser independiente** del que gestiona el turno `DEP-I10`, y **`Q-32` sigue abierta**: no está decidido cómo se detecta la caída.
*Naturaleza:* **decisión pendiente**, no problema duro ni desconocimiento del equipo.

### M-1 · Modificabilidad — (A, B)

**A (Importancia).** El documento de reglas está en **v3.0 Borrador** `TEN-E` y ya cambió al menos dos veces antes de que exista el sistema; si cada ajuste de parámetro costara un ciclo de desarrollo, con **dos personas y 14 semanas** `ORG-01`, `ORG-05` se pagaría en funcionalidad no entregada.

**B (Dificultad).** **Ya está resuelto**: `D-30` puso los tres conjuntos de parámetros en tablas de solo lectura, `seed-configuration.sql` existe y está cargado, y `Game.Services` los pasa al dominio como argumentos sin violar `E 2.2`.
*Naturaleza:* trabajo hecho. Lo que falta es aplicarlo, no descubrir cómo.

### F-1 · Corrección funcional — (A, A)

**A (Importancia).** Si la puntuación no corresponde a lo jugado, **el juego no es el juego**: la puntuación parcial decide quién coloca el rey en la ronda siguiente `DEP-I6`, así que un error no se queda quieto, **se propaga al resto de la partida**. Es la razón por la que `E 2.1` concentró toda la autoridad en el servidor.

**A (Dificultad).** Atraviesa la **cadena más larga del dominio** —`DEP-I6 → DEP-I7 → DEP-I2 → DEP-I3`—, sobre castillos cuya superficie **crece durante toda la partida** `DR-23`, con una condición de exclusividad por castillo, con las piezas del retirado saliendo del tablero y sus construcciones quedándose `DR-25`, `DR-32`, y con un **empate resuelto al azar** `R 2.3` que choca con las pruebas deterministas `TEN-F`.
*Naturaleza:* **el problema es duro**. No es que el equipo no sepa: es que el cálculo depende de un estado geométrico acumulado y de una fuente de azar que hay que domesticar sin romper `E 2.2`.

### T-1 · Testabilidad — (A, A)

**A (Importancia).** No es una comodidad del equipo: **`E 2.2` justifica con ella toda la dirección de dependencias**. Si las reglas no pueden probarse aisladas, la estructura en cuatro capas pierde su razón declarada, y `E 10` —régimen de pruebas obligatorio, exigible por STK-03— queda incumplido justo en lo que `OBJ-01` dice que se evalúa: el código.

**A (Dificultad).** Hay **dos fuentes de azar** en el dominio —el sorteo del empate `R 2.3` y la obtención de cartas `DR-06`— y `E 10.4` exige aserciones directas sin lógica en la prueba. Hacerlas comprobables obliga a **cambiar cómo el dominio expresa esas reglas**, convirtiendo el azar en un colaborador sustituible, y a hacerlo con dos personas de cinco sombreros `TEN-H`.
*Naturaleza:* **el problema es duro**, no desconocimiento. La técnica es conocida; lo que cuesta es que la regla escrita invoca el azar directamente y hay que reformularla sin traicionarla.

### L-1 · Analizabilidad — (M, B)

**M (Importancia).** Lo pierde **STK-02 en su sombrero de operador**, no el jugador, y **no cambia ninguna estructura**: es disciplina al escribir el mensaje. Cuando falla, el coste es reproducir el fallo —molesto y a veces imposible `D-21`—, pero recuperable.

**B (Dificultad).** El estándar ya dice exactamente qué hacer: registrador por clase `E 9.1`, operación e identificador de la entidad `E 9.3`, un solo registro por fallo `E 9.5`, excepción como argumento. `log4net` lo hace de fábrica.
*Naturaleza:* **ni duro ni desconocido.** La única sombra es `C-3` —verificar log4net sobre .NET 10—, que es una **comprobación pendiente**, no una dificultad.

---

## 4. Comprobación contra los criterios de calibración

| Criterio | Cumplimiento |
|---|---|
| Máximo dos hojas en (A, A) | **Exactamente dos**: F-1 y T-1 |
| Una línea de justificación por cada letra | Diez líneas para cinco hojas ✔ |
| Si es difícil, declarar si el problema es duro o falta conocimiento | Declarado en las cinco: dos *problema duro*, una *decisión pendiente*, dos *ni duro ni desconocido* |
| Ninguna hoja es un adjetivo | Las cinco son escenarios de seis partes, con medida refutable ✔ |
| Ningún refinamiento es una solución | Ninguno nombra caché, réplica, motor de reglas ni configuración externa. Todos enuncian el **problema** ✔ |
| Las categorías no vienen de un catálogo | Salen de CON-01…CON-10; 25010 solo pone el nombre `DR-16` ✔ |
| Toda hoja tiene procedencia | Las cinco citan su `CON-` y su stakeholder ✔ |

---

## 5. Lo que el árbol muestra sin decirlo

**Cinco renglones vacíos, y cada uno significa algo distinto:**

| Renglón sin hoja | Qué significa |
|---|---|
| Disponibilidad · caída del servidor | **Está listo para escribirse.** Tiene las mejores medidas del proyecto —frontera de turno, 2 jugadores con cuenta, 3 min—. Solo faltó por el encargo de cinco escenarios |
| Modificabilidad · cambio que alcanza al dominio | **Es la variante cara, y sigue sin cubrirse.** Cambiar lo que el rey otorga por ronda o la fórmula de puntuación obliga a tocar `Game.Domain` igualmente. Está declarado en CON-02, no olvidado |
| Corrección funcional · aislamiento entre partidas | **Está listo**, con invariante de 5 partidas `DR-31`. Repetía categoría con F-1 |
| Seguridad · confidencialidad | **No se escribió a propósito.** Su escenario tendría que declarar que su alcance excluye el tránsito, y eso depende de **`C-2`**, que STK-02 dejó sin definir |
| Seguridad · autenticidad | Igual que el anterior |

**La categoría Seguridad está entera vacía.** Es lo más visible del árbol: hay dos concerns adoptados —CON-06 y CON-10, con siete decisiones detrás— y **ninguna hoja debajo**. No es descuido: es la consecuencia directa de dejar `C-2` sin definir, y el árbol lo enseña.

## 6. Qué se ve al mirar el árbol entero

**La Importancia no discrimina; la Dificultad sí.** Cuatro de cinco hojas son A en importancia, y no es por no decidir: cuatro tienen **efecto profundo** —cambian o justifican la estructura del sistema— y L-1 no lo tiene, y por eso es M. El filtro real lo hace la segunda letra: A, A, M, B, B.

**Las dos (A, A) comparten causa.** F-1 y T-1 son duras **por lo mismo**: `TEN-F`. Las reglas escritas invocan el azar en dos sitios —`R 2.3` y `DR-06`— y eso hace a la vez difícil calcular de forma reproducible y difícil probar. **Una sola decisión de diseño —convertir la fuente de azar en un colaborador sustituible del dominio— desbloquea las dos hojas más caras del árbol.** Es el primer sitio donde mirar.

**Nada quedó marcado difícil por desconocimiento.** Las dos dificultades altas son del problema; la media es una decisión pendiente (`Q-32`); las dos bajas son trabajo ya resuelto o ya especificado.
