reset
cr

\ include ../flib/spi/lora1276.fs
\ 4335  6 rf-init

spi-init

\ ===== faster SPI

ssel @ io-base GPIO.BSRR + variable spi.addr
ssel @ io# variable spi.bit
: +spi ( -- ) $10000 spi.bit @ lshift spi.addr @ ! inline ;  \ select SPI
: -spi ( -- )      1 spi.bit @ lshift spi.addr @ ! inline ;  \ deselect SPI

: spi1-dr ( spi1-sr -- spi1-dr ) 4 + inline ;
: spi-txrdy ( spi1-sr -- spi1-sr ) begin dup @ 2 and until inline ;
: spi-rxrdy ( spi1-sr -- spi1-sr ) begin dup @ 1 and until inline ;
: spi-rxdrop ( spi1-sr -- spi1-sr ) begin dup @ 1 and until dup spi1-dr @ drop inline ;
: spi-push ( c spi1-sr -- spi1-sr ) swap over spi1-dr ! inline ;
: spi-push0 ( spi1-sr -- spi1-sr ) 0 over spi1-dr ! inline ;

: >spi2 ( c reg -- ) \ write register
  swap $80 or swap
  +spi SPI1-SR ( c reg spi1-sr )
  spi-push spi-rxdrop
  spi-push spi-rxrdy
  spi1-dr @ drop
  -spi
  ;

: spi2> ( reg -- c ) \ read register
  +spi SPI1-SR ( reg spi1-sr )
  spi-push spi-rxdrop ( spi1-sr )
  spi-push0 spi-rxrdy ( spi1-sr )
  spi1-dr @ ( c )
  -spi
  ;

: >spiN ( addr len reg -- ) \ write len bytes to reg
  +spi
  SPI1-SR spi-push ( addr len spi1-sr )
  swap 0 ?do
    over c@ ( addr spi1-sr c )
    swap spi-rxdrop ( addr c spi1-sr )
    spi-push ( addr spi1-sr )
    swap 1+ swap ( addr+1 spi1-sr )
  loop
  nip spi-rxdrop drop -spi
  ;

: spiN> ( addr len reg -- ) \ read len bytes from reg
  +spi
  SPI1-SR spi-push spi-rxdrop ( addr len spi1-sr )
  swap 0 ?do ( addr spi1-sr )
    spi-push0
    spi-rxrdy
    dup spi1-dr @ ( addr spi1-sr c )
    rot dup 1+ ( spi1-sr c addr addr+1 )
    -rot c! ( spi1-sr addr+1 )
    swap
  loop
  2drop -spi
  ;



\ ===== radio

: rf!@ ( b reg -- b ) +spi >spi >spi> -spi ; \ perform a 2-byte SPI cycle
: rf! ( b reg -- ) $80 or rf!@ drop ;        \ write register
: rf@ ( reg -- b ) 0 swap rf!@ ;             \ read register
: rf-n@spi ( addr len -- )                   \ read N bytes from the FIFO
  0 do
    0 0 +spi >spi> drop >spi> drop -spi      \ read 1 byte from FIFO
    over c! 1+
  loop drop ;
: rf-n!spi ( addr len -- )                   \ write N bytes to the FIFO
  0 do dup c@
    0 $80 +spi >spi> drop >spi> drop -spi    \ write 1 byte to FIFO
  1+ loop drop ;

$88 1 rf! \ set LoRa mode & sleep
$39 constant RF:SYNC


\ ===== tests

: dt micros swap - 66 - 50 + 100 / . ;

256 buffer: data
: data-init 256 0 do i data i + c! loop ; data-init

: t ." rf! : " micros 100 0 do i RF:SYNC rf! loop dt ; t t t
: t ." rf!@: " micros 100 0 do i $B9 rf!@ drop loop dt ; t t t
: t 2 0 do  i RF:SYNC rf!  i $39 >spi2  i $39 >spi2  loop ;

: r 2 0 do  RF:SYNC rf@  $39 spi2>  $39 spi2> + + loop + . ;

: tn ." >spiN: " micros data 256 0 >spiN micros swap - 66 - 10 * 256 / . ;
: rn ." spiN>: " micros data 256 0 spiN> micros swap - 66 - 10 * 256 / . ;

