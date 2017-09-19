\ burn SMA Solar app to flash memory

<<<core>>>
cr

compiletoflash
( app start: ) here dup hex.

include sma.fs
include sma2.fs
include lcd.fs
include app.fs

( app end, size: ) here dup hex. swap - .
