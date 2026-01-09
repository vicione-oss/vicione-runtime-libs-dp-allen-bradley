# Allen‑Bradley Logix (EtherNet/IP) – Adressierung

für Logix (ControlLogix/CompactLogix/GuardLogix/SoftLogix) über EtherNet/IP. 


## EBNF – Grammatik

```ebnf
ROOT        = "DataPort Allen-Bradley Logix – EtherNet/IP" ;
ROOT        -> DEVICE+ ;

DEVICE      = <DeviceName> ;
DEVICE      -> SECTION* ;                               // Abschnitte optional, beliebige Reihenfolge

SECTION     = SYSTEM | MODULES | CONTROLLER | PROGRAMS ;

// System (Diagnose)
SYSTEM      = "System" ;
SYSTEM      -> NODE* ;                                  // Nur System Tags erlaubt

// Module-I/O (module-defined)
MODULES     = "Modules" ;
MODULES     -> (LOCAL | REMOTE)* ;                      // max. je 1x (Validierungsregel)

LOCAL       = "Local" ;
LOCAL       -> SLOT* ;

REMOTE      = "Remote" ;
REMOTE      -> ADAPTER* ;

ADAPTER     = <AdapterName> ;                           // Name aus der Logix-I/O-Konfiguration
ADAPTER     -> SLOT* | IO_SETS? ;                       // Chassis hinter Adapter → SLOT*, sonst direkt IO_SETS

SLOT        = <SlotNumber> ;                            // 0..n
SLOT        -> IO_SETS ;

IO_SETS     = CONFIG?, INPUT? , OUTPUT? ;               // Reihenfolge wie angegeben

CONFIG      = "Config" ;
CONFIG      -> NODE* ;

INPUT       = "Input" ;
INPUT       -> NODE* ;

OUTPUT      = "Output" ;
OUTPUT      -> NODE* ;

// Controller- und Program-Scope
CONTROLLER  = "Tags" ;                                  // Controller-Scope
CONTROLLER  -> NODE* ;

PROGRAMS    = "Programs" ;
PROGRAMS    -> PROGRAM* ;

PROGRAM     = <ProgramName> ;                           // inkl. Program-Parametern
PROGRAM     -> NODE* ;

NODE        = SCALAR | STRUCT | ARRAY ;

SCALAR      = <Name> ;                                  // BOOL,(U)SINT,(U)INT,(U)DINT,(U)LINT,REAL,LREAL,STRING,TIME,LTIME,…
SCALAR      -> /* leaf */ ;

STRUCT      = <Name> ;                                  // Instanz eines Strukturtyps (UDT, AOI, vordef. TIMER/COUNTER/AXIS_*/MSG,…)
STRUCT      -> NODE* ;                                  // rekursiv

ARRAY       = <Name> ;
ARRAY       -> INDEX+ ;                                 // 1..k Dimensionen

INDEX       = <nonneg_integer> ;                        // 0..N gemäß Deklaration in der PLC
INDEX       -> NODE ;                                   // Array-Element (Skalar/Struct/Array)

// Bits sind keine Knoten – nur Adress-Suffix .bit auf ganzzahligen Skalaren
// SINT/USINT: 0..7; INT/UINT: 0..15; DINT/UDINT: 0..31; LINT/ULINT: 0..63
```

## Semantik & Regeln

* **Geltung:** Nur Logix (ControlLogix/CompactLogix/GuardLogix/SoftLogix) mit symbolischer Tag-Adressierung.

* **Struct vereinheitlicht:** UDT, AOI und vordef. Strukturen laufen einheitlich als `STRUCT`; konkreter Typ in opt. Metadaten.

**Arrays**
Mehrdimensionale Arrays erzeugen mehrere `INDEX`‑Ebenen. Das Array‑Element (`INDEX -> NODE`) kann wiederum Skalar, Struct oder weiteres Array sein.

**Bits**
Bitzugriff ausschließlich als **Suffix** `.n` auf atomaren Ganzzahl‑Skalaren (`SINT/INT/DINT/LINT`). Keine Bits auf `BOOL` oder `ARRAY OF BOOL`.

**String**
Extern ein **Leaf**. Interne Felder (`LEN`, `DATA[]`) sind nur für Diagnose und werden nicht exponiert.

**Node**
`In/Out/InOut` über die Transferrichtung relisieren. 

* **Program-Parameter:** erscheinen unter `PROGRAM/<Name>` wie normale Tags; Metadatum `paramDirection: "In"|"Out"|"InOut"`.

* **Namensregeln:** In reinen Namen keine Trenner `.` `[]` `:`. UI-Escaping nach Bedarf.

* **Optionalität:** Abschnitte können fehlen (z. B. keine Remote-I/O, keine Programs).

* **Validierung:**
  - Geschwister eindeutig.
  - In MODULES je max. 1× Local und Remote.
  - In IO_SETS erscheint jeder der Knoten Input/Output/Config höchstens 1×.
  - dims ↔ Anzahl INDEX-Ebenen konsistent.
  - Bitbereiche einhalten.