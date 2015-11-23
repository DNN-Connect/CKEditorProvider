﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ElFinder
{
    internal static class Mime
    {
        const string mimeslist = @"
application/andrew-inset ez
application/chemtool cht
application/dicom dcm
application/docbook+xml docbook
application/ecmascript ecma
application/flash-video flv
application/illustrator ai
application/javascript js
application/mac-binhex40
application/mathematica nb
application/msword doc
application/octet-stream bin
application/oda oda
application/ogg ogg
application/pdf pdf
application/pgp pgp
application/pgp-encrypted
application/pgp-encrypted pgp gpg
application/pgp-keys
application/pgp-keys skr pkr
application/pgp-signature
application/pgp-signature sig
application/pkcs7-mime
application/pkcs7-signature p7s
application/postscript ps
application/rtf rtf
application/sdp sdp
application/smil smil smi sml
application/stuffit sit
application/vnd.corel-draw cdr
application/vnd.hp-hpgl hpgl
application/vnd.hp-pcl pcl
application/vnd.lotus-1-2-3 123 wk1 wk3 wk4 wks
application/vnd.mozilla.xul+xml xul
application/vnd.ms-excel xls xlc xll xlm xlw xla xlt xld
application/vnd.ms-powerpoint ppz ppt pps pot
application/vnd.oasis.opendocument.chart odc
application/vnd.oasis.opendocument.database odb
application/vnd.oasis.opendocument.formula odf
application/vnd.oasis.opendocument.graphics odg
application/vnd.oasis.opendocument.graphics-template otg
application/vnd.oasis.opendocument.image odi
application/vnd.oasis.opendocument.presentation odp
application/vnd.oasis.opendocument.presentation-template otp
application/vnd.oasis.opendocument.spreadsheet ods
application/vnd.oasis.opendocument.spreadsheet-template ots
application/vnd.oasis.opendocument.text odt
application/vnd.oasis.opendocument.text-master odm
application/vnd.oasis.opendocument.text-template ott
application/vnd.oasis.opendocument.text-web oth
application/vnd.palm pdb
application/vnd.rn-realmedia
application/vnd.rn-realmedia rm
application/vnd.rn-realmedia-secure rms
application/vnd.rn-realmedia-vbr rmvb
application/vnd.stardivision.calc sdc
application/vnd.stardivision.chart sds
application/vnd.stardivision.draw sda
application/vnd.stardivision.impress sdd sdp
application/vnd.stardivision.mail smd
application/vnd.stardivision.math smf
application/vnd.stardivision.writer sdw vor sgl
application/vnd.sun.xml.calc sxc
application/vnd.sun.xml.calc.template stc
application/vnd.sun.xml.draw sxd
application/vnd.sun.xml.draw.template std
application/vnd.sun.xml.impress sxi
application/vnd.sun.xml.impress.template sti
application/vnd.sun.xml.math sxm
application/vnd.sun.xml.writer sxw
application/vnd.sun.xml.writer.global sxg
application/vnd.sun.xml.writer.template stw
application/vnd.wordperfect wpd
application/x-abiword abw abw.CRASHED abw.gz zabw
application/x-amipro sam
application/x-anjuta-project prj
application/x-applix-spreadsheet as
application/x-applix-word aw
application/x-arc
application/x-archive a
application/x-arj arj
application/x-asax asax
application/x-ascx ascx
application/x-ashx ashx
application/x-asix asix
application/x-asmx asmx
application/x-asp asp
application/x-awk
application/x-axd axd
application/x-bcpio bcpio
application/x-bittorrent torrent
application/x-blender blender blend BLEND
application/x-bzip bz bz2
application/x-bzip bz2 bz
application/x-bzip-compressed-tar tar.bz tar.bz2
application/x-bzip-compressed-tar tar.bz tar.bz2 tbz tbz2
application/x-cd-image iso
application/x-cgi cgi
application/x-chess-pgn pgn
application/x-chm chm
application/x-class-file
application/x-cmbx cmbx
application/x-compress Z
application/x-compressed-tar tar.gz tar.Z tgz taz
application/x-compressed-tar tar.gz tgz
application/x-config config
application/x-core
application/x-cpio cpio
application/x-cpio-compressed cpio.gz
application/x-csh csh
application/x-cue cue
application/x-dbase dbf
application/x-dbm
application/x-dc-rom dc
application/x-deb deb
application/x-designer ui
application/x-desktop desktop kdelnk
application/x-devhelp devhelp
application/x-dia-diagram dia
application/x-disco disco
application/x-dvi dvi
application/x-e-theme etheme
application/x-egon egon
application/x-executable exe
application/x-font-afm afm
application/x-font-bdf bdf
application/x-font-dos
application/x-font-framemaker
application/x-font-libgrx
application/x-font-linux-psf psf
application/x-font-otf
application/x-font-pcf pcf
application/x-font-pcf pcf.gz
application/x-font-speedo spd
application/x-font-sunos-news
application/x-font-tex
application/x-font-tex-tfm
application/x-font-ttf ttc TTC
application/x-font-ttf ttf
application/x-font-type1 pfa pfb gsf pcf.Z
application/x-font-vfont
application/x-frame
application/x-frontline aop
application/x-gameboy-rom gb
application/x-gdbm
application/x-gdesklets-display display
application/x-genesis-rom gen md
application/x-gettext-translation gmo
application/x-glabels glabels
application/x-glade glade
application/x-gmc-link
application/x-gnome-db-connection connection
application/x-gnome-db-database database
application/x-gnome-stones caves
application/x-gnucash gnucash gnc xac
application/x-gnumeric gnumeric
application/x-graphite gra
application/x-gtar gtar
application/x-gtktalog
application/x-gzip gz
application/x-gzpostscript ps.gz
application/x-hdf hdf
application/x-ica ica
application/x-ipod-firmware
application/x-jamin jam
application/x-jar jar
application/x-java class
application/x-java-archive jar ear war
application/x-jbuilder-project jpr jpx
application/x-karbon karbon
application/x-kchart chrt
application/x-kformula kfo
application/x-killustrator kil
application/x-kivio flw
application/x-kontour kon
application/x-kpovmodeler kpm
application/x-kpresenter kpr kpt
application/x-krita kra
application/x-kspread ksp
application/x-kspread-crypt
application/x-ksysv-package
application/x-kugar kud
application/x-kword kwd kwt
application/x-kword-crypt
application/x-lha lha lzh
application/x-lha lzh
application/x-lhz lhz
application/x-linguist ts
application/x-lyx lyx
application/x-lzop lzo
application/x-lzop-compressed-tar tar.lzo tzo
application/x-macbinary
application/x-machine-config
application/x-magicpoint mgp
application/x-master-page master
application/x-matroska mkv
application/x-mdp mdp
application/x-mds mds
application/x-mdsx mdsx
application/x-mergeant mergeant
application/x-mif mif
application/x-mozilla-bookmarks
application/x-mps mps
application/x-ms-dos-executable exe
application/x-mswinurl
application/x-mswrite wri
application/x-msx-rom msx
application/x-n64-rom n64
application/x-nautilus-link
application/x-nes-rom nes
application/x-netcdf cdf nc
application/x-netscape-bookmarks
application/x-object o
application/x-ole-storage
application/x-oleo oleo
application/x-palm-database
application/x-palm-database pdb prc
application/x-par2 PAR2 par2
application/x-pef-executable
application/x-perl pl pm al perl
application/x-php php php3 php4
application/x-pkcs12 p12 pfx
application/x-planner planner mrproject
application/x-planperfect pln
application/x-prjx prjx
application/x-profile
application/x-ptoptimizer-script pto
application/x-pw pw
application/x-python-bytecode pyc pyo
application/x-quattro-pro wb1 wb2 wb3
application/x-quattropro wb1 wb2 wb3
application/x-qw qif
application/x-rar rar
application/x-rar-compressed rar
application/x-rdp rdp
application/x-reject rej
application/x-remoting rem
application/x-resources resources
application/x-resourcesx resx
application/x-rpm rpm
application/x-ruby
application/x-sc
application/x-sc sc
application/x-scribus sla sla.gz scd scd.gz
application/x-shar shar
application/x-shared-library-la la
application/x-sharedlib so
application/x-shellscript sh
application/x-shockwave-flash swf
application/x-siag siag
application/x-slp
application/x-smil kino
application/x-smil smi smil
application/x-sms-rom sms gg
application/x-soap-remoting soap
application/x-streamingmedia ssm
application/x-stuffit
application/x-stuffit bin sit
application/x-sv4cpio sv4cpio
application/x-sv4crc sv4crc
application/x-tar tar
application/x-tarz tar.Z
application/x-tex-gf gf
application/x-tex-pk k
application/x-tgif obj
application/x-theme theme
application/x-toc toc
application/x-toutdoux
application/x-trash   bak old sik
application/x-troff tr roff t
application/x-troff-man man
application/x-troff-man-compressed
application/x-tzo tar.lzo tzo
application/x-ustar ustar
application/x-wais-source src
application/x-web-config
application/x-wpg wpg
application/x-wsdl wsdl
application/x-x509-ca-cert der cer crt cert pem
application/x-xbel xbel
application/x-zerosize
application/x-zoo zoo
application/xhtml+xml xhtml
application/zip zip
audio/ac3 ac3
audio/basic au snd
audio/midi mid midi
audio/mpeg mp3
audio/prs.sid sid psid
audio/vnd.rn-realaudio ra
audio/x-aac aac
audio/x-adpcm
audio/x-aifc
audio/x-aiff aif aiff
audio/x-aiff aiff aif aifc
audio/x-aiffc
audio/x-flac flac
audio/x-m4a m4a
audio/x-mod mod ult uni XM m15 mtm 669
audio/x-mp3-playlist
audio/x-mpeg
audio/x-mpegurl m3u
audio/x-ms-asx
audio/x-pn-realaudio ra ram rm
audio/x-pn-realaudio ram rmm
audio/x-riff
audio/x-s3m s3m
audio/x-scpls pls
audio/x-scpls pls xpl
audio/x-stm stm
audio/x-voc voc
audio/x-wav wav
audio/x-xi xi
audio/x-xm xm
image/bmp bmp
image/cgm cgm
image/dpx
image/fax-g3 g3
image/g3fax
image/gif gif
image/ief ief
image/jpeg jpeg jpg jpe
image/jpeg2000 jp2
image/png png
image/rle rle
image/svg+xml svg
image/tiff tif tiff
image/vnd.djvu djvu djv
image/vnd.dwg dwg
image/vnd.dxf dxf
image/x-3ds 3ds
image/x-applix-graphics ag
image/x-cmu-raster ras
image/x-compressed-xcf xcf.gz xcf.bz2
image/x-dcraw bay BAY bmq BMQ cr2 CR2 crw CRW cs1 CS1 dc2 DC2 dcr DCR fff FFF k25 K25 kdc KDC mos MOS mrw MRW nef NEF orf ORF pef PEF raf RAF rdc RDC srf SRF x3f X3F
image/x-dib
image/x-eps eps epsi epsf
image/x-fits fits
image/x-fpx
image/x-icb icb
image/x-ico ico
image/x-iff iff
image/x-ilbm ilbm
image/x-jng jng
image/x-lwo lwo lwob
image/x-lws lws
image/x-msod msod
image/x-niff
image/x-pcx
image/x-photo-cd pcd
image/x-pict pict pict1 pict2
image/x-portable-anymap pnm
image/x-portable-bitmap pbm
image/x-portable-graymap pgm
image/x-portable-pixmap ppm
image/x-psd psd
image/x-rgb rgb
image/x-sgi sgi
image/x-sun-raster sun
image/x-tga tga
image/x-win-bitmap cur
image/x-wmf wmf
image/x-xbitmap xbm
image/x-xcf xcf
image/x-xfig fig
image/x-xpixmap xpm
image/x-xwindowdump xwd
inode/blockdevice
inode/chardevice
inode/directory
inode/fifo
inode/mount-point
inode/socket
inode/symlink
message/delivery-status
message/disposition-notification
message/external-body
message/news
message/partial
message/rfc822
message/x-gnu-rmail
model/vrml wrl
multipart/alternative
multipart/appledouble
multipart/digest
multipart/encrypted
multipart/mixed
multipart/related
multipart/report
multipart/signed
multipart/x-mixed-replace
text/calendar vcs ics
text/css css CSSL
text/directory vcf vct gcrd
text/enriched
text/html html htm
text/htmlh
text/mathml mml
text/plain txt asc
text/rdf rdf
text/rfc822-headers
text/richtext rtx
text/rss rss
text/sgml sgml sgm
text/spreadsheet sylk slk
text/tab-separated-values tsv
text/vnd.rn-realtext rt
text/vnd.wap.wml wml
text/x-adasrc adb ads
text/x-authors
text/x-bibtex bib
text/x-boo boo
text/x-c++hdr hh
text/x-c++src cpp cxx cc C c++
text/x-chdr h h++ hp
text/x-comma-separated-values csv
text/x-copying
text/x-credits
text/x-csrc c
text/x-dcl dcl
text/x-dsl dsl
text/x-dsrc d
text/x-dtd dtd
text/x-emacs-lisp el
text/x-fortran f
text/x-gettext-translation po
text/x-gettext-translation-template pot
text/x-gtkrc
text/x-haskell hs
text/x-idl idl
text/x-install
text/x-java java
text/x-js js
text/x-ksysv-log
text/x-literate-haskell lhs
text/x-log log
text/x-makefile
text/x-moc moc
text/x-msil il
text/x-nemerle n
text/x-objcsrc m
text/x-pascal p pas
text/x-patch diff patch
text/x-python py
text/x-readme
text/x-rng rng
text/x-scheme scm
text/x-setext etx
text/x-speech
text/x-sql sql
text/x-suse-ymp ymp
text/x-suse-ymu ymu
text/x-tcl tcl tk
text/x-tex tex ltx sty cls
text/x-texinfo texi texinfo
text/x-texmacs tm ts
text/x-troff-me me
text/x-troff-mm mm
text/x-troff-ms ms
text/x-uil uil
text/x-uri uri url
text/x-vb vb
text/x-xds xds
text/x-xmi xmi
text/x-xsl xsl
text/x-xslfo fo xslfo
text/x-xslt xslt xsl
text/xmcd
text/xml xml
video/3gpp 3gp
video/dv dv dif
video/isivideo
video/mpeg mpeg mpg mp2 mpe vob dat
video/quicktime qt mov moov qtvr
video/vivo
video/vnd.rn-realvideo rv
video/wavelet
video/x-3gpp2 3g2
video/x-anim anim[1-9j]
video/x-avi
video/x-flic fli flc
video/x-mng mng
video/x-ms-asf asf asx
video/x-ms-wmv wmv
video/x-msvideo avi
video/x-nsv nsv NSV
video/x-real-video
video/x-sgi-movie movie
application/x-java-jnlp-file      jnlp
application/vnd.openxmlformats-officedocument.wordprocessingml.document docx
application/vnd.openxmlformats-officedocument.wordprocessingml.template dotx
application/vnd.ms-word.document.macroEnabled.12 docm
application/vnd.ms-word.template.macroEnabled.12 dotm
application/vnd.openxmlformats-officedocument.spreadsheetml.sheet xlsx
application/vnd.openxmlformats-officedocument.spreadsheetml.template xltx
application/vnd.ms-excel.sheet.macroEnabled.12 xlsm
application/vnd.ms-excel.template.macroEnabled.12 xltm
application/vnd.ms-excel.addin.macroEnabled.12 xlam
application/vnd.ms-excel.sheet.binary.macroEnabled.12 xlsb
application/vnd.openxmlformats-officedocument.presentationml.presentation pptx
application/vnd.openxmlformats-officedocument.presentationml.template potx
application/vnd.openxmlformats-officedocument.presentationml.slideshow ppsx
application/vnd.ms-powerpoint.addin.macroEnabled.12 ppam
";
        private static Dictionary<string, string> _mimeTypes;
        static Mime()
        {
            _mimeTypes = new Dictionary<string, string>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                string[] names = assembly.GetManifestResourceNames();

                using (StringReader reader = new StringReader(mimeslist))
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;
                        line = line.Trim();
                        if (!string.IsNullOrWhiteSpace(line) && line[0] != '#')
                        {
                            string[] parts = line.Split(' ');
                            if (parts.Length > 1)
                            {
                                string mime = parts[0];
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    string ext = parts[i].ToLower();
                                    if (!_mimeTypes.ContainsKey(ext))
                                    {
                                        _mimeTypes.Add(ext, mime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static string GetMimeType(string extension)
        {
            extension = extension.ToLower();
            if (_mimeTypes.ContainsKey(extension))
            {
                return _mimeTypes[extension];
            }
            else
            {
                return "unknown";
            }
        }
    }
}
