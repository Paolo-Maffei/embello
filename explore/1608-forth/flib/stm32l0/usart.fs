\ hardware UART driver

[ifndef] TX2  PA2 constant TX2  [then]
[ifndef] RX2  PA3 constant RX2  [then]

$40004400 constant USART2
     USART2 $00 + constant USART2-CR1
     USART2 $04 + constant USART2-CR2
     USART2 $08 + constant USART2-CR3
     USART2 $0C + constant USART2-BRR \ baud rate register
     USART2 $10 + constant USART2-GTPR \ guard time and prescaler register
     USART2 $1C + constant USART2-ISR \ interrupt status register
     USART2 $20 + constant USART2-ICR \ interrupt flag clear register
     USART2 $24 + constant USART2-RDR \ receive data register
     USART2 $28 + constant USART2-TDR \ transmit data register

: usart2. ( -- )
  USART2
  cr ."     CR1 " dup @ hex. 4 +
     ."     CR2 " dup @ hex. 4 +
     ."     CR3 " dup @ hex. 4 +
     ."     BRR " dup @ hex. 4 +
  cr ."    GTPR " dup @ hex. 12 +
     ."     ISR " dup @ hex. 4 +
     ."     ICR " dup @ hex. drop ;

\ RX/TX ready checks
: usart2>? ( -- f ) \ can a byte be read?
  5 bit USART2-ISR bit@ ;
: >usart2? ( -- f ) \ can a byte be transmited?
  7 bit USART2-ISR bit@ ;

\ single byte transfers
: usart2> ( -- c ) \ read byte from USART
  begin usart2>? until USART2-RDR @ ;
: >usart2 ( c -- ) \ write byte to USART
  begin >usart2? until USART2-TDR ! ;

: usart2-baud ( n -- ) \ set baud rate
  dup 2/ 16000000 + swap / USART2-BRR ! ; \ assumes 16Mhz clock

: usart2-init ( -- )  \ set up hardware USART
  OMODE-AF-PP TX2   io-mode!
  OMODE-AF-PP RX2   io-mode!
  TX2 io-base GPIO.AFRL + dup @ $ff00 bic $4400 or swap ! \ assumes PA2&PA3

  17 bit RCC-APB1ENR bis!  \ enable clock: set USART2EN
  $0 USART2-CR1 ! \ make sure it's disabled
  $0 USART2-CR2 !
  $0 USART2-CR3 !
  9600 usart2-baud
  $00121b5f USART2-ICR ! \ clear all status flags
  USART2-RDR @ drop \ clear rx register
  $D USART2-CR1 ! \ tx-en, rx-en, uart-en
  ;
