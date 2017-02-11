\ core libraries

<<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/mecrisp/disassembler-m0.fs

: rf-send ; \ to make varint happy even without radio driver
include ../flib/any/buffers.fs
\ include ../flib/spi/rf69.fs
include ../flib/spi/lora1276.fs
include ../flib/any/varint.fs
\ include gps.fs


( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
