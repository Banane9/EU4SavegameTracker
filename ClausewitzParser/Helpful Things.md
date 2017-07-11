# Helpful Things

* For removing all non-special entries from Ironmelt binary token file  
    `^.*(?<!Special);.{0,3}\r\n`

* For removing all special entries from Ironmelt binary token file  
    `.*Special.*\r\n`

* For removing all empty entries from Ironmelt binary token file  
    `.*;;;\r\n`

* For stripping the extra data from the entries in the Ironmelt binary token file  
    `(?<=.*;.*);.*\r\n`

* [EU4 (Clausewitz) Savegame Format](http://eu4parser.readthedocs.io/en/latest/format/#the-binary-serialization)

* 