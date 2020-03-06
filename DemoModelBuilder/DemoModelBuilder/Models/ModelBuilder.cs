////////////////////////////////////////////////////////////////////////////////////////////////////
// CONFIDENCIAL:
// Este es un archivo de código fuente propiedad de RSolver SpA y está protegido bajo derechos
// de autor y leyes de protección a la propiedad intelectual. Ud. no debe ver, revisar, copiar,
// utilizar, compartir, o distribuir este archivo, excepto que cuente con la expresa licencia y/o
// autorización de RSolver SpA.
// CONFIDENTIAL:
// This is a source code file property of RSolver SpA and is protected by copyright laws.
// You may not see, read, check, copy, share, and/or distribute this file without the written
// authorization from RSolver SpA.
////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright © 2020 R:Solver (RSolver SpA)
//  Todos los derechos reservados sobre este código fuente / All rights reserved over this source code
//
//  ModelBuilder.cs
//  Methods to build classification models using ML.NET.
//
//  .NET Project:       Demos
//  Company Project:    Demos
//  Creado/Created:     feb 2020 - Rodrigo Sandoval (rodrigo.sandoval@rsolver.com)
//  
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Linq;
using Microsoft.ML;

namespace DemoModelBuilder.Models
{
    public class ModelBuilder
    {
        // private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        public static string _basePath = @"C:\Users\rodri\Documents\GitHub\RDemos\DemoModelBuilder\DemoModelBuilder";
        const string dataFolder = "data";
        
        private static string _sampleDataPath => Path.Combine(_basePath, dataFolder, "SampleComments.tsv");
        private static string _modelPath => Path.Combine(_basePath, dataFolder, "CustomerCommentModel.zip");

        private static MLContext _mlContext;
        private static PredictionEngine<CustomerMessage, MessageTypePrediction> _predEngine;
        private static ITransformer _trainedModel;
        static IDataView _samplesDataView;

        public ModelBuilder()
        { }

        public void Build()
        {
            _mlContext = new MLContext(seed: 0);

            _samplesDataView = _mlContext.Data.LoadFromTextFile<CustomerMessage>(_sampleDataPath, hasHeader: true);

            DataOperationsCatalog.TrainTestData dataSplit = _mlContext.Data.TrainTestSplit(_samplesDataView, testFraction: 0.2);
            IDataView _trainingDataView = dataSplit.TrainSet;
            IDataView _testDataView = dataSplit.TestSet;

            var pipeline = ProcessData();

            var trainingPipeline = BuildAndTrainModel(_trainingDataView, pipeline);

            Evaluate(_trainingDataView.Schema, _testDataView);

            PredictDocument();
        }
        public static IEstimator<ITransformer> ProcessData()
        {
            Console.WriteLine($"=============== Processing Data ===============");
            // STEP 2: Common data process configuration with pipeline data transformations
            // <SnippetMapValueToKey>
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Type", outputColumnName: "Label")
                            // </SnippetMapValueToKey>
                            // <SnippetFeaturizeText>
                            .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Contents", outputColumnName: "ContentsFeaturized"))
                            // </SnippetFeaturizeText>
                            // <SnippetConcatenate>
                            // .Append(_mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized"))
                            // </SnippetConcatenate>
                            //Sample Caching the DataView so estimators iterating over the data multiple times, instead of always reading from file, using the cache might get better performance.
                            // <SnippetAppendCache>
                            .AppendCacheCheckpoint(_mlContext);
            // </SnippetAppendCache>

            Console.WriteLine($"=============== Finished Processing Data ===============");

            // <SnippetReturnPipeline>
            return pipeline;
            // </SnippetReturnPipeline>
        }

        public static IEstimator<ITransformer> BuildAndTrainModel(IDataView trainingDataView, IEstimator<ITransformer> pipeline)
        {
            // STEP 3: Create the training algorithm/trainer
            // Use the multi-class SDCA algorithm to predict the label using features.
            // Set the trainer/algorithm and map label to value (original readable state)
            // <SnippetAddTrainer> 
            var trainingPipeline =
                pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "ContentsFeaturized"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            // </SnippetAddTrainer> 

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine($"=============== Training the model  ===============");

            // <SnippetTrainModel> 
            _trainedModel = trainingPipeline.Fit(trainingDataView);
            // </SnippetTrainModel> 
            Console.WriteLine($"=============== Finished Training the model Ending time: {DateTime.Now.ToString()} ===============");

            // (OPTIONAL) Try/test a single prediction with the "just-trained model" (Before saving the model)

            // Create prediction engine related to the loaded trained model
            // <SnippetCreatePredictionEngine1>
            _predEngine = _mlContext.Model.CreatePredictionEngine<CustomerMessage, MessageTypePrediction>(_trainedModel);
            // </SnippetCreatePredictionEngine1>

            // Original type: 2 ALTA
            CustomerMessage sample = new CustomerMessage();
            sample.Contents = "Necesito que por favor habiliten el numero +99 9 8999 9009  para realizar llamadas internacionales, principalmente Brasil";
            var prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // Original type: 3 BAJA 
            sample.Contents = "Estimados  Favor solicito bajar servicio , Roaming ,Larga distancia internacional y SMS ( Solo emisión ) a los celulares";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // Original type: 9 Reclamo
            sample.Contents = "esta mañana realicé req 999999, el cual ustedes lo finalizaron, adjuntando un pdf, el requerimiento fue otro, el cual indica que la plataforma no funciona, ¿cuando estará disponible?";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // Original type: 9 RECLAMO
            sample.Contents = "Buen día tengo problemas de comunicación no se puede realizar ni recibir llamados es urgente las lineas son";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // <SnippetReturnModel>
            return trainingPipeline;
            // </SnippetReturnModel>

        }

        public static void Evaluate(DataViewSchema trainingDataViewSchema, IDataView testDataView)
        {
            // STEP 5:  Evaluate the model in order to get the model's accuracy metrics
            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Starting time: {DateTime.Now.ToString()} ===============");


            //Evaluate the model on a test dataset and calculate metrics of the model on the test data.
            // <SnippetEvaluate>
            var testMetrics = _mlContext.MulticlassClassification.Evaluate(_trainedModel.Transform(testDataView));
            // </SnippetEvaluate>

            Console.WriteLine($"=============== Evaluating to get model's accuracy metrics - Ending time: {DateTime.Now.ToString()} ===============");
            // <SnippetDisplayMetrics>
            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:0.###}");
            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:#.###}");
            Console.WriteLine($"*************************************************************************************************************");
            // </SnippetDisplayMetrics>

            // Save the new model to .ZIP file
            // <SnippetCallSaveModel>
            SaveModelAsFile(_mlContext, trainingDataViewSchema, _trainedModel);
            // </SnippetCallSaveModel>

        }

        public static void PredictDocument()
        {
            // <SnippetLoadModel>
            ITransformer loadedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
            // </SnippetLoadModel>

            // <SnippetAddTestIssue> 
            // Correct Type: 5
            CustomerMessage sample = new CustomerMessage() { Contents = "Favor realizar cambio de plan a los abonados:  989990999 / 999999090  Plan actual ZVB, favor cambiar plan y dejar activo plan ZVC de 8GB.  Agradecido. Saludos. SA" };
            // </SnippetAddTestIssue> 

            //Predict label for single hard-coded issue
            // <SnippetCreatePredictionEngine>
            _predEngine = _mlContext.Model.CreatePredictionEngine<CustomerMessage, MessageTypePrediction>(loadedModel);
            // </SnippetCreatePredictionEngine>

            // <SnippetPredictIssue>
            var prediction = _predEngine.Predict(sample);
            // </SnippetPredictIssue>

            // <SnippetDisplayResults>
            Console.WriteLine($"=============== Single Prediction - Result: {prediction.TypeCode} ===============");
            // </SnippetDisplayResults>

        }

        private static void SaveModelAsFile(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
        {
            // <SnippetSaveModel> 
            mlContext.Model.Save(model, trainingDataViewSchema, _modelPath);
            // </SnippetSaveModel>

            Console.WriteLine("The model is saved to {0}", _modelPath);
        }
    }
}
