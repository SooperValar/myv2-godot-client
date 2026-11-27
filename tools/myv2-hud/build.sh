#!/bin/sh
# HUD art for MyV2, always from TLG's originals: eighth attribute row, wider value columns, Planner and Update tabs,
# the equipment boxes as MyV2 uses them (Back where TLG's Card box was, Belt where Back was, Legs where Belt was),
# and a Help button under Hide.
set -e
python3 /tmp/hud/extend.py && python3 /tmp/hud/columns.py && python3 /tmp/hud/tabs.py
for n in 00001 18001; do python3 /tmp/hud/slots.py /mnt/user-data/outputs/hud/$n.png /mnt/user-data/outputs/hud/$n.png; done
python3 /tmp/hud/helpbtn.py /mnt/user-data/outputs/hud/00001.png /mnt/user-data/outputs/hud/00001.png
python3 /tmp/hud/helpbtn.py /mnt/user-data/outputs/hud/18001.png /mnt/user-data/outputs/hud/18001.png --dark
