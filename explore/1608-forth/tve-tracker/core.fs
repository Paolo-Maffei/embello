\ core libraries

<<<board>>>
cr compiletoflash
( core start: ) here dup hex.

\ ===== Hardware configuration of nodes

\ Possible values for configuration variables
69   CONSTANT rfm69
96   CONSTANT rfm96
1276 CONSTANT drf1276
1231 CONSTANT sx1231
1276 CONSTANT sx1276
$a6  CONSTANT pa6h

\ Configuration variables
0 VARIABLE has:radio
0 VARIABLE has:radio-chip
0 VARIABLE has:gps

compiletoram \ we don't need the has>... words in flash, nor the conftab

\ RADIO module options
: has>rfm69 rfm69     has:radio !         \ the node has a HopeRF rfm69 radio
            sx1231    has:radio-chip ! ;
: has>rfm96 rfm96     has:radio !         \ the node has a HopeRF rfm96 radio
            sx1276    has:radio-chip ! ;
: has>drf1276 drf1276 has:radio !         \ the node has a Dorji drf1276 radio
            sx1276    has:radio-chip ! ;

\ GPS module options
: has>pa6h  pa6h      has:gps ! ;         \ the node has a globaltop PA6H / firefly 1 GPS

\ Node configuration table
: conftab case hwid
$39440C4E of has>rfm96 has>pa6h endof \ prototype tracker
$12345678 of has>rfm69 has>pa6h endof
endcase ;
conftab

compiletoflash

\ ===== End of hardware configuration

include ../flib/any/buffers.fs
\ include ../flib/spi/rf69.fs
\ include ../flib/spi/lora1276.fs
\ include ../flib/any/varint.fs
include ../flib/mecrisp/disassembler-m0.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
