# SMA Solar Inverter readout

This is a project to read out a solar inverter from SMA (model 5000TL in this
case) via Bluetooth. It displays current power, today's yield, and total
producrtion on an attached 16x2 character LCD. The display updates every 10
seconds.

For details, see the posts on the JeeLabs weblog:
[part 1](http://jeelabs.org/2017/10/sma-solar-readout---part-1/) -
[part 2](http://jeelabs.org/2017/10/sma-solar-readout---part-2/) -
[part 3](http://jeelabs.org/2017/10/sma-solar-readout---part-3/).

Built with an STM32F103 board, a soldering iron, Mecrisp Forth, OpenSCAD, and an
UltiMaker.
