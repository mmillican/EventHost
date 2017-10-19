/// <binding ProjectOpened='default' />
"use strict";

var gulp = require("gulp"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    htmlmin = require("gulp-htmlmin"),
    uglify = require("gulp-uglify"),
    merge = require("merge-stream"),
    del = require("del"),
    bundleconfig = require("./bundleconfig.json"),
    sass = require('gulp-sass'),
    prefix = require('gulp-autoprefixer');

var regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/
};

var paths = {
    cssSrc: 'wwwroot/css',
    sassSrc: 'wwwroot/sass',
    jsSrc: 'wwwroot/js',
    cssDist: 'wwwroot/css/',
    jsDist: 'wwwroot/js/',
    imgSrc: 'wwwroot/images/'
};

gulp.task("min", ["min:js", "min:css", "min:html"]);

gulp.task('compile:css', function () {
    return gulp.src(paths.sassSrc + '/*.scss')
        .pipe(sass({
            //outputStyle: 'compressed'
        })).on('error', sass.logError)
        .pipe(prefix())
        .pipe(gulp.dest(paths.cssDist))
});

gulp.task("min:js", function () {
    var tasks = getBundles(regex.js).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(uglify())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:css", function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(cssmin())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:html", function () {
    var tasks = getBundles(regex.html).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(htmlmin({ collapseWhitespace: true, minifyCSS: true, minifyJS: true }))
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("clean", function () {
    var files = bundleconfig.map(function (bundle) {
        return bundle.outputFileName;
    });

    return del(files);
});

gulp.task("watch", function () {
   gulp.watch(paths.sassSrc + '/**/*.scss', ["compile:css"]);
});

function getBundles(regexPattern) {
    return bundleconfig.filter(function (bundle) {
        return regexPattern.test(bundle.outputFileName);
    });
}

gulp.task("default", ["watch"]);