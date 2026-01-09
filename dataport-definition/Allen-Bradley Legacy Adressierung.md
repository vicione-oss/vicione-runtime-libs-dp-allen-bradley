# Allen‑Bradley Legacy PLC/SLC/MicroLogix (EtherNet/IP) – Adressierung

für **PLC‑5 / SLC‑500 / MicroLogix** über EtherNet/IP. 
*datei-/wortbasierte Adressierung* (z. B. `N7:0`, `I:1/0`, `T4:0.PRE`)

## EBNF – Grammatik

```ebnf
ROOT          = "DataPort Allen‑Bradley Classic – PLC‑5/SLC/MicroLogix" ;
ROOT          -> DEVICE+ ;

DEVICE        = <DeviceName> ;
DEVICE        -> FILES ;

FILES         = "Files" ;
FILES         -> FILETYPE+ ;

FILETYPE      = <FileCode> ;          // I,O,S,B,N,F,L,T,C,R,ST,PD, …
FILETYPE      -> FILENUM* ;

FILENUM       = <FileNumber> ;        // dezimal (z. B. N7 → FileCode=N, FileNumber=7)
FILENUM       -> ELEMENT* ;

ELEMENT       = <ElementNumber> ;     // Wort/Element (z. B. N7:0 , B3:12 , I:1.0)
ELEMENT       -> NODE* ;              // für strukturierte Dateien (T,C,R,ST,PD), sonst leaf

// Einheitliches Datenmodell für strukturierte Dateitypen
NODE          = WORD | STRUCT | ARRAY ;

WORD          = <WordName> ;          // z. B. "Word" oder Unterwortnummer; meist leaf
WORD          -> /* leaf */ ;

STRUCT        = <StructName> ;        // z. B. T (Timer), C (Counter), R (Control), ST (String), PD (PID)
STRUCT        -> FIELD* ;

FIELD         = <FieldName> ;         // z. B. PRE, ACC, EN, DN, TT, LEN, POS, DATA, …
FIELD        -> (ARRAYDATA | LEAF) ;

ARRAY         = <ArrayName> ;         // falls ein Filed selbst ein Array ist (z. B. ST.DATA[])
ARRAY         -> INDEX+ ;

ARRAYDATA     = "DATA" ;              // spezielles Feld für String‑Daten
ARRAYDATA     -> INDEX+ ;

INDEX         = <nonneg_integer> ;    // 0..N
INDEX         -> LEAF ;

LEAF          = <Name> ;              // Endpunkt (numerisch/bitfähig)
LEAF          -> /* leaf */ ;

// Bits sind keine Knoten; Bitadressierung als Suffix erlaubt:
// - Wort-/Statusbit:   N7:0/3 , B3:12/0 , I:1.0/7
// - Feldbits:          T4:0/EN  (Alias zu T4:0.EN)
// - Unterwort + Bit:   T4:0.PRE/0  (Alias zu T4:0.1/0 bei Plattformen, die das unterstützen)
```

### Hinweise & Validierung

* **FileCode** identifiziert Dateityp (z. B. `N`=Integer, `B`=Binary, `F`=Float, `I`=Input, `O`=Output, `S`=Status, `T`=Timer, `C`=Counter, `R`=Control, `ST`=String, `PD`=PID …).
* **Element** ist in N/B/F/L die **Wortnummer**; bei I/O zusätzlich Slot/Word (z. B. `I:1.0`).
* **Strukturdateien** (T/C/R/ST/PD …) besitzen **Felder** (Timer: `PRE, ACC, EN, TT, DN`; Counter: `PRE, ACC, CU, CD, DN, OV, UN, UA`; String: `LEN`, `DATA[]`).
* **Bit‑Suffix** (`/n`) ist auf **Wörter und Status-/Felder** zulässig.

#### Mapping Baum ↔ Adresse

* Baum: `…/Files/N/7/0`  →  `N7:0`
* Baum: `…/Files/T/4/0/PRE`  →  `T4:0.PRE` (gleichwertig zu `T4:0/EN` nur für Statusfelder; PRE/ACC sind Wortfelder ohne EN/DN‑Semantik)
* Baum: `…/Files/I/1/0`  →  `I:1.0` ; Bit `.7` → `I:1.0/7`


# Allen‑Bradley Legacy Micro800 (EtherNet/IP) – Adressierung

für **Micro800** tag-/symbolbasierte Adressierung (Native Tags wie bei IEC‑Variablen; keine AOIs).


```ebnf
ROOT          = "DataPort Allen‑Bradley Micro800 – EtherNet/IP" ;
ROOT          -> DEVICE+ ;

DEVICE        = <DeviceName> ;
DEVICE        -> CONTROLLER , PROGRAMS? ;               // Globale + lokale Variablen

CONTROLLER    = "Tags" ;                                // globale (Controller‑Scope) Variablen
CONTROLLER    -> NODE* ;

PROGRAMS      = "Programs" ;
PROGRAMS      -> PROGRAM* ;

PROGRAM       = <ProgramName> ;
PROGRAM       -> NODE* ;                                // lokale Variablen des Programms

// Einheitliches Datenmodell (keine AOIs)
NODE          = SCALAR | STRUCT | ARRAY ;

SCALAR        = <Name> ;                                // BOOL, (U)INT, (U)DINT, (U)LINT, REAL, LREAL, STRING, …
SCALAR        -> /* leaf */ ;

STRUCT        = <Name> ;                                // UDT/Strukturinstanz
STRUCT        -> NODE* ;

ARRAY         = <Name> ;
ARRAY         -> INDEX+ ;

INDEX         = <nonneg_integer> ;
INDEX         -> NODE ;

// Bits: nur Suffix auf ganzzahligen Skalaren (plattformabhängig vom Treiber unterstützt)
```

### Hinweise & Normalisierung

* **Adresse = Tagname** (Native Tag). Member per `.` und Indizes per `[i]`.
* **I/O** erscheinen als vordefinierte/controller‑weite Tags (herstellerabhängige Namenskonventionen).
* **Keine AOIs**; UDTs/Strukturen sind erlaubt.


## Validierungsregeln (gemeinsam)

1. Geschwister eindeutig benannt.
2. **Legacy**: `FileCode` gültig; `Filenum` ≥ 0; `Element` ≥ 0.
3. **Legacy**: Feldnamen nur in strukturierten Dateien zulässig; Bit‑Suffix nur auf Wort-/Statusfeldern.
4. **Micro800**: `STRUCT` muss Typinfo enthalten (optional in Metadaten).
5. Bitbereiche passend zum Datentyp einhalten.