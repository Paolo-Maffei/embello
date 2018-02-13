\ Cached I/O to internal flash memory, saves only to flash when needed.

$00010000 constant FDISK-BASE

\ return the number of bits needed to represent the specified unsigned integer
\ i.e. floor(log2(v))+1, examples: 64..127 => 7, 128..255 => 8, 256..511 => 9
: bitwidth ( u -- u ) clz 32 swap - 1-foldable ;

FDISK-BASE flash-pagesize constant F-SIZE
       F-SIZE 1- bitwidth constant F-BITS

F-SIZE cell+ buffer: f-buf

\ 4-byte prefix 0 = page bits, 1 = dirty counter, 2-3 = current page
: cache-init ( a u -- )  swap ! ;

: f-dirty ( -- u ) f-buf 1+ c@ ;   \ return dirty flag, or 0 if cache is clean
: f-page  ( -- u ) f-buf 2+ h@ ;   \ return current page in cache buffer

: f-putpage ( addr page -- )  \ internal flash write, always exactly one block
  F-SIZE * FDISK-BASE +  ( src dst )
  dup flashpageerase 
  dup F-SIZE + swap  ( src dst-hi dst-lo )
  do
    dup h@ i hflash!
    2+
  2 +loop  drop ;

: f-read ( addr len pos -- )  \ internal flash read, can cross page boundaries
  FDISK-BASE + -rot move ;

: f-getpage ( addr page -- )  \ internal flash read, always exactly one block
  F-SIZE swap over * f-read
;

: f-load ( u -- )  \ load page into cache
  f-buf cell+ over f-getpage
  f-buf 2+ h! ;  \ adjust cache page

: f-save ( -- )  \ load cache data to flash
  f-buf cell+ f-buf 2+ h@ f-putpage
  0 f-buf 1+ c! ;  \ clear dirty flag

: f-flush ( -- )  \ flush page when its time has come
  f-dirty ?dup if       \ is the cache dirty?
    1- dup f-buf 1+ c!  \ decrement dirty flag
    if f-save then      \ if it reached zero, save the data
  then ;

: f-write ( addr len pos -- )  \ internal flash write can cross page boundaries
  -rot over + swap do  ( pos )                \ loop over each byte to write
    dup F-SIZE /  ( pos page )                \ convert position to page
    dup f-page <> if                          \ not same page as in cache?
      f-dirty if f-save then                  \ save if cache is dirty
      dup f-load                              \ load new block into cache
    then  drop
    i c@ over F-SIZE 1- and f-buf cell+ + c!  \ save single byte into cache
    $FF f-buf 1+ c!                           \ mark cache "very" dirty
    1+
  loop  drop ;

: f-init ( -- )  \ init flash cache buffer
  f-buf F-BITS cache-init
  f-buf cell+ F-SIZE 0 f-read  \ pre-load cache with page 0 data
;
