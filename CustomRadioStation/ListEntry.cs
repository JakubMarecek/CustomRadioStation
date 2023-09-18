/* 
 * Custom Radio Station
 * Copyright (C) 2021  Jakub Mareček (info@jakubmarecek.cz)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Custom Radio Station.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace CustomRadioStation
{
    public class ListEntry
    {
        public string FileName { get; set; }

        public string Name { get; set; }

        public float Volume { get; set; }

        public int Duration { get; set; }

        public string Condition { get; set; }

        public string ConditionR { get; set; }

        public string Tooltip { get; set; }
    }
}
