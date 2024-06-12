# install font needed for run and debug
echo "Installing 'Hack' font from github@source-foundry/Hack..."
curl -L -O https://github.com/source-foundry/Hack/releases/download/v3.003/Hack-v3.003-ttf.tar.gz
tar -xzvf Hack-v3.003-ttf.tar.gz
mkdir ~/.local/share/fonts
mv ttf/Hack-Regular.ttf ~/.local/share/fonts/Hack-Regular.ttf
mv ttf/Hack-Italic.ttf ~/.local/share/fonts/Hack-Italic.ttf
mv ttf/Hack-Bold.ttf ~/.local/share/fonts/Hack-Bold.ttf
mv ttf/Hack-BoldItalic.ttf ~/.local/share/fonts/Hack-BoldItalic.ttf
rm -rf ttf && rm Hack-v3.003-ttf.tar.gz
echo "regenerating font cache"
fc-cache -f -v

# replace font target in Content/Fonts/Default.spritefont
sed -i.bak -E 's/ChevyRay - Softsquare Mono/\1'"Hack"'/' /Content/Fonts/Default.spritefont
