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

using System;
using System.IO;
using Gibbed.IO;

namespace CustomRadioStation
{
	public class WEMFile
	{
        private long _fmt_offset = -1, _cue_offset = -1, _LIST_offset = -1, _smpl_offset = -1, _vorb_offset = -1, _data_offset = -1;
        private uint _sample_rate = 0, _avg_bytes_per_second = 0, _subtype = 0,
            _cue_count = 0, _loop_count = 0, _loop_start = 0, _loop_end = 0, _sample_count = 0, _setup_packet_offset = 0, _first_audio_packet_offset = 0, _uid = 0;
        private ushort _channels = 0, _ext_unk = 0;
        private bool _little_endian = true, _no_granule = false, _mod_packets = false, force_packet_format = false, kForceNoModPackets = false, kForceModPackets = false,
			_header_triad_present = false, _old_packet_headers = false, _inline_codebooks = false, _full_setup = false;
        private int _fmt_size = -1, _cue_size = -1, _LIST_size = -1, _smpl_size = -1, _vorb_size = -1, _data_size = -1, _blocksize_0_pow = 0, _blocksize_1_pow = 0;
		string _codebooks_name = "";

		public string ParseErrorStr { private set; get; }

		public int AudioLength { private set; get; }

        public ushort Channels { get { return _channels; } }

        public bool LoadWEM(string file)
        {
			if (!file.EndsWith(".wem"))
			{
				ParseErrorStr = "not a wem file";
				return false;
			}

            var f = File.Open(file, FileMode.Open);
            long _file_size = f.Length;
            int _riff_size = -1;

            {
                string riff_head = f.ReadValueHead(); // RIFF, RIFX

                if (riff_head != "RIFX")
                {
                    if (riff_head != "RIFF")
                    {
                        ParseErrorStr = ("missing RIFF");
						f.Close();
						return false;
                    }
                    else
                    {
                        _little_endian = true;
                    }
                }
                else
                {
                    _little_endian = false;
                }

                _riff_size = f.ReadValueS32() + 8;

                if (_riff_size > _file_size)
				{
					ParseErrorStr = ("RIFF truncated");
					f.Close();
					return false;
				}

                string wave_head = f.ReadValueHead();
                if (wave_head != "WAVE")
				{
					ParseErrorStr = ("missing WAVE");
					f.Close();
					return false;
				}
            }

            // read chunks
            long chunk_offset = 12;
            while (chunk_offset < _riff_size)
            {
                f.Seek(chunk_offset, SeekOrigin.Begin);

                if (chunk_offset + 8 > _riff_size) 
				{
					ParseErrorStr = ("chunk header truncated");
					f.Close();
					return false;
				}

                string chunk_type = f.ReadValueHead();
                int chunk_size;

                chunk_size = f.ReadValueS32();

                if (chunk_type == "fmt ")
                {
                    _fmt_offset = chunk_offset + 8;
                    _fmt_size = chunk_size;
                }
                else if (chunk_type == "cue ")
                {
                    _cue_offset = chunk_offset + 8;
                    _cue_size = chunk_size;
                }
                else if (chunk_type == "LIST")
                {
                    _LIST_offset = chunk_offset + 8;
                    _LIST_size = chunk_size;
                }
                else if (chunk_type == "smpl")
                {
                    _smpl_offset = chunk_offset + 8;
                    _smpl_size = chunk_size;
                }
                else if (chunk_type == "vorb")
                {
                    _vorb_offset = chunk_offset + 8;
                    _vorb_size = chunk_size;
                }
                else if (chunk_type == "data")
                {
                    _data_offset = chunk_offset + 8;
                    _data_size = chunk_size;
                }

                chunk_offset = chunk_offset + 8 + chunk_size;
            }

            if (chunk_offset > _riff_size) 
			{
				ParseErrorStr = ("chunk truncated");
				f.Close();
				return false;
			}

            // check that we have the chunks we're expecting
            if (-1 == _fmt_offset && -1 == _data_offset) 
			{
				ParseErrorStr = ("expected fmt, data chunks");
				f.Close();
				return false;
			}
        
            // read fmt
            if (-1 == _vorb_offset && 0x42 != _fmt_size) 
			{
				ParseErrorStr = ("expected 0x42 fmt if vorb missing");
				f.Close();
				return false;
			}
        
            if (-1 != _vorb_offset && 0x28 != _fmt_size && 0x18 != _fmt_size && 0x12 != _fmt_size) 
			{
				ParseErrorStr = ("bad fmt size");
				f.Close();
				return false;
			}
        
            if (-1 == _vorb_offset && 0x42 == _fmt_size)
            {
                // fake it out
                _vorb_offset = _fmt_offset + 0x18;
            }

            f.Seek(_fmt_offset, SeekOrigin.Begin);
            if (0xffff != f.ReadValueU16()) 
			{
				ParseErrorStr = ("bad codec id");
				f.Close();
				return false;
			}
            _channels = f.ReadValueU16();
            _sample_rate = f.ReadValueU32();
            _avg_bytes_per_second = f.ReadValueU32();
			AudioLength = (int)Math.Floor((double)(_file_size / _avg_bytes_per_second));

            if (0U != f.ReadValueU16()) 
			{
				ParseErrorStr = ("bad block align");
				f.Close();
				return false;
			}
            if (0U != f.ReadValueU16()) 
			{
				ParseErrorStr = ("expected 0 bps");
				f.Close();
				return false;
			}
            if (_fmt_size-0x12 != f.ReadValueU16()) 
			{
				ParseErrorStr = ("bad extra fmt length");
				f.Close();
				return false;
			}

            if (_fmt_size-0x12 >= 2) {
              // read extra fmt
              _ext_unk = f.ReadValueU16();
              if (_fmt_size-0x12 >= 6) {
                _subtype = f.ReadValueU32();
              }
            }
            
            if (_fmt_size == 0x28)
            {
                byte[] whoknowsbuf;
                byte[] whoknowsbuf_check = new byte[16] {1,0,0,0, 0,0,0x10,0, 0x80,0,0,0xAA, 0,0x38,0x9b,0x71};
                whoknowsbuf = f.ReadBytes(16);
                if (whoknowsbuf != whoknowsbuf_check) 
				{
					ParseErrorStr = ("expected signature in extra fmt?");
					f.Close();
					return false;
				}
            }

            // read cue
            if (-1 != _cue_offset)
            {
                //if (0x1c != _cue_size) throw Parse_error_str("bad cue size");

                f.Seek(_cue_offset, SeekOrigin.Begin);

                _cue_count = f.ReadValueU32();
            }
            
            // read LIST
            if (-1 != _LIST_offset)
            {
                /*if ( 4 != _LIST_size ) throw Parse_error_str("bad LIST size");
                char adtlbuf[4];
                const char adtlbuf_check[4] = {'a','d','t','l'};
                _infile.seekg(_LIST_offset);
                _infile.read(adtlbuf, 4);
                if (memcmp(adtlbuf, adtlbuf_check, 4)) throw Parse_error_str("expected only adtl in LIST");*/
            }

            // read smpl
            if (-1 != _smpl_offset)
            {
                f.Seek(_smpl_offset+0x1C, SeekOrigin.Begin);
                _loop_count = f.ReadValueU32();

                if (1 != _loop_count) 
				{
					ParseErrorStr = ("expected one loop");
					f.Close();
					return false;
				}

                f.Seek(_smpl_offset+0x2c, SeekOrigin.Begin);
                _loop_start = f.ReadValueU32();
                _loop_end = f.ReadValueU32();
            }
            
            // read vorb
            switch (_vorb_size)
            {
                case -1:
                case 0x28:
                case 0x2A:
                case 0x2C:
                case 0x32:
                case 0x34:
                    f.Seek(_vorb_offset+0x00, SeekOrigin.Begin);
                    break;

                default:
                    ParseErrorStr = ("bad vorb size");
					f.Close();
					return false;
            }

            _sample_count = f.ReadValueU32();

            switch (_vorb_size)
            {
                case -1:
                case 0x2A:
                {
                    _no_granule = true;

                    f.Seek(_vorb_offset+0x4, SeekOrigin.Begin);
                    uint mod_signal = f.ReadValueU32();

                    // set
                    // D9     11011001
                    // CB     11001011
                    // BC     10111100
                    // B2     10110010
                    // unset
                    // 4A     01001010
                    // 4B     01001011
                    // 69     01101001
                    // 70     01110000
                    // A7     10100111 !!!

                    // seems to be 0xD9 when _mod_packets should be set
                    // also seen 0xCB, 0xBC, 0xB2
                    if (0x4A != mod_signal && 0x4B != mod_signal && 0x69 != mod_signal && 0x70 != mod_signal)
                    {
                        _mod_packets = true;
                    }
                    f.Seek(_vorb_offset+0x10, SeekOrigin.Begin);
                    break;
                }

                default:
                    f.Seek(_vorb_offset+0x18, SeekOrigin.Begin);
                    break;
            }

            if (force_packet_format == kForceNoModPackets)
            {
                _mod_packets = false;
            }
            else if (force_packet_format == kForceModPackets)
            {
                _mod_packets = true;
            }

            _setup_packet_offset = f.ReadValueU32();
            _first_audio_packet_offset = f.ReadValueU32();

            switch (_vorb_size)
            {
                case -1:
                case 0x2A:
                    f.Seek(_vorb_offset+0x24, SeekOrigin.Begin);
                    break;

                case 0x32:
                case 0x34:
                    f.Seek(_vorb_offset+0x2C, SeekOrigin.Begin);
                    break;
            }

            switch(_vorb_size)
            {
                case 0x28:
                case 0x2C:
                    // ok to leave _uid, _blocksize_0_pow and _blocksize_1_pow unset
                    _header_triad_present = true;
                    _old_packet_headers = true;
                    break;

                case -1:
                case 0x2A:
                case 0x32:
                case 0x34:
                    _uid = f.ReadValueU32();
                    _blocksize_0_pow = f.ReadByte();
                    _blocksize_1_pow = f.ReadByte();
                    break;
            } 

            // check/set loops now that we know total sample count
            if (0 != _loop_count)
            {
                if (_loop_end == 0)
                {
                    _loop_end = _sample_count;
                }
                else
                {
                    _loop_end = _loop_end + 1;
                }

                if (_loop_start >= _sample_count || _loop_end > _sample_count || _loop_start > _loop_end)
                {
					ParseErrorStr = ("loops out of range");
					f.Close();
					return false;
				}
            }

            // check subtype now that we know the vorb info
            // this is clearly just the channel layout
            switch (_subtype)
            {
                case 4:     /* 1 channel, no seek table */
                case 3:     /* 2 channels */
                case 0x33:  /* 4 channels */
                case 0x37:  /* 5 channels, seek or not */
                case 0x3b:  /* 5 channels, no seek table */
                case 0x3f:  /* 6 channels, no seek table */
                    break;
                default:
                    //throw Parse_error_str("unknown subtype");
                    break;
            }

            f.Close();

			return true;
        }

		public string PrintInfo()
		{
			string info = "";
			string endl = Environment.NewLine;

    		if (_little_endian)
    		{
    		    info += "RIFF WAVE";
    		}
    		else
    		{
    		    info += "RIFX WAVE";
    		}
    		info += " " + _channels + " channel";
    		if (_channels != 1) info += "s";
    		info += " " + _sample_rate + " Hz " + _avg_bytes_per_second*8 + " bps" + endl;
    		info += _sample_count + " samples" + endl;

    		if (0 != _loop_count)
    		{
    		    info += "loop from " + _loop_start + " to " + _loop_end + endl;
    		}

    		if (_old_packet_headers)
    		{
    		    info += "- 8 byte (old) packet headers" + endl;
    		}
    		else if (_no_granule)
    		{
    		    info += "- 2 byte packet headers, no granule" + endl;
    		}
    		else
    		{
    		    info += "- 6 byte packet headers" + endl;
    		}

    		if (_header_triad_present)
    		{
    		    info += "- Vorbis header triad present" + endl;
    		}

    		if (_full_setup || _header_triad_present)
    		{
    		    info += "- full setup header" + endl;
    		}
    		else
    		{
    		    info += "- stripped setup header" + endl;
    		}

    		if (_inline_codebooks || _header_triad_present)
    		{
    		    info += "- inline codebooks" + endl;
    		}
    		else
    		{
    		    info += "- external codebooks (" + _codebooks_name + ")" + endl;
    		}

    		if (_mod_packets)
    		{
    		    info += "- modified Vorbis packets" + endl;
    		}
    		else
    		{
    		    info += "- standard Vorbis packets" + endl;
    		}

    		/*if (0 != _cue_count)
    		{
    		    cout << _cue_count << " cue point";
    		    if (_cue_count != 1) cout << "s";
    		    cout << endl;
    		}*/

			return info;
		}
	}
}