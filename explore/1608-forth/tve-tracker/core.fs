\ core libraries

<<<board>>>
cr compiletoflash
( core start: ) here dup hex.

\ include ../flib/mecrisp/disassembler-m0.fs

include ../flib/stm32l0/uart2.fs
include ../flib/any/ring.fs
include ../flib/stm32l0/uart2-irq.fs

include ../flib/any/buffers.fs
include ../flib/spi/lora1276.fs
include ../flib/any/varint.fs
include ../tlib/numprint.fs

include gps.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
