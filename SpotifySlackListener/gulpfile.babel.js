import { src, lastRun, dest, task, watch } from 'gulp';

import sass from 'gulp-sass';
import cssmin from 'gulp-cssmin';
import autoprefixer from 'gulp-autoprefixer';
import plumber from 'gulp-plumber';

const paths = {
    webroot: 'wwwroot',
    assets: 'Assets'
};

paths.css = `${paths.assets}/css/**/*.scss`;
paths.cssDest = `${paths.webroot}/css/`;

function css() {
    return src(paths.css, {
        base: `./${paths.assets}/css/`
    })
    .pipe(plumber())
    .pipe(sass())
    .pipe(cssmin({ keepSpecialComments: 0}))
    .pipe(autoprefixer({cascade: false}))
    .pipe(dest(paths.cssDest));
}

task('watch', () => {
    watch(paths.css, css)
});

task('min', css);