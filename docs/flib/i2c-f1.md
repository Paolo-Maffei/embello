# Hardware I2C

[code]: stm32f1/i2c.fs ( io cond hal ring )
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f1/i2c.fs">stm32f1/i2c.fs</a>
* Needs: io, cond, hal, ring

Perform I2C communication (following i2c-bb protocol) using the hardware driver I2C1.

Each I2C transaction consists of the following steps:

    start the transaction by calling i2c-addr
    send all the bytes out with repeated calls to >i2c (or none at all)
    give the number of expected bytes read back to i2c-xfer (can be 0)
    check the result to verify that the device responded (false means ok)
    read the reply bytes with repeated calls to i2c> (or none at all)
    the transaction will be closed by the driver when the count is reached

The driver performs the transaction using DMA, in order to avoid several issues with the hardware. `i2c>` and it's variants will wait until the data has been received completely before returning. One may also poll `i2c.rxbuf ring#` if this is not desireable.

### API

[defs]: <> (i2c-init i2c-addr >i2c i2c-xfer i2c> i2c>h i2c>h_inv i2c-fast i2c-standard i2c. )
```
: i2c-init     ( -- )   \ Init and reset I2C. Default to 100 kHz
: i2c-addr     ( u --)  \ Start a new transaction
: >i2c         ( u -- ) \ Queues byte u for transmission over i2c. Use after i2c-addr
: i2c-xfer     ( n -- nak ) \ Prepares for reading an nbyte reply.
: i2c>         ( -- u ) \ Receives 1 byte from i2c. Use after i2c-xfer. Waits.
: i2c>h        ( -- u ) \ Receives 16 bit word from i2c, lsb first.
: i2c>h_inv    ( -- u ) \ Receives 16 bit word from i2c, msb first.
: i2c-fast     ( -- )   \ Configure I2C for fast mode (~400kHz)
: i2c-standard ( -- )   \ Configure I2C for Standard Mode (~100kHz)
: i2c.         ( -- )   \ scan and report all I2C devices on the bus
```

### Constants

The driver reserves 2x this size + 8 bytes for the incoming and outgoing data.

[defs]: <> (i2c-bufsize)
```
[ifndef] i2c-bufsize 128 constant i2c-bufsize [then]
```


It does not make much sense updating these, without also updating the code to use the alternate pins.

[defs]: <> (SCL SDA)
```
[ifndef] SCL  PB6 constant SCL  [then]
[ifndef] SDA  PB7 constant SDA  [then]
```

### Examples

OLED Display on bus address $3C:

```
lcd-init i2c-fast
show-logo
```

PCF8574 on bus address $3F (formally, this chip only supports standard mode):

```
i2c-init
: send10 10 0 do i >i2c loop ;
$3f i2c-addr  send10 0 i2c-xfer
```


