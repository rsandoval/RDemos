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
//  BankModelBuilder.cs
//  Methods to build classification model for a Bank Messaging context, using ML.NET.
//
//  .NET Project:       Demos
//  Company Project:    Demos
//  Creado/Created:     mar 2020 - Rodrigo Sandoval (rodrigo.sandoval@rsolver.com)
//  
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Linq;
using Microsoft.ML;

namespace DemoModelBuilder.Models
{
    public class BankMessageTypeModelBuilder
    {
        public static string _basePath = @"C:\Users\rodri\Documents\GitHub\RDemos\DemoModelBuilder\DemoModelBuilder";
        const string dataFolder = "data";

        private static string _sampleDataPath => Path.Combine(_basePath, dataFolder, "SampleBankComments.tsv");
        private static string _modelPath => Path.Combine(_basePath, dataFolder, "BankCommentTypeModel.zip");

        private static MLContext _mlContext;
        private static PredictionEngine<BankCustomerMessage, BankMessageTypePrediction> _predEngine;
        private static ITransformer _trainedModel;
        static IDataView _samplesDataView;

        public BankMessageTypeModelBuilder()
        { }

        public void Build()
        {
            _mlContext = new MLContext(seed: 0);

            _samplesDataView = _mlContext.Data.LoadFromTextFile<BankCustomerMessage>(_sampleDataPath, hasHeader: true);

            DataOperationsCatalog.TrainTestData dataSplit = _mlContext.Data.TrainTestSplit(_samplesDataView, testFraction: 0.2);
            IDataView _trainingDataView = dataSplit.TrainSet;
            IDataView _testDataView = dataSplit.TestSet;

            var pipeline = ProcessData();

            var trainingPipeline = BuildAndTrainModel(_trainingDataView, pipeline);

            Evaluate(_trainingDataView.Schema, _testDataView);

            PredictComment();
        }
        public static IEstimator<ITransformer> ProcessData()
        {
            Console.WriteLine($"=============== Processing Data ===============");
            // STEP 2: Common data process configuration with pipeline data transformations
            // <SnippetMapValueToKey>
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "TypeCode", outputColumnName: "Label")
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
            _predEngine = _mlContext.Model.CreatePredictionEngine<BankCustomerMessage, BankMessageTypePrediction>(_trainedModel);
            // </SnippetCreatePredictionEngine1>

            // Original type: 1 (Consulta sobre productos y Servicios / Cuenta Corriente y Servicios)
            BankCustomerMessage sample = new BankCustomerMessage();
            sample.Contents = "Estimada buenas tardes, porque se generó pago de mínimo tarjeta de crédito este mes si a finales de junio la pagué?";
            var prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // Original type: 5 (Consulta sobre Productos y Servicios / Productos de inversión)
            sample.Contents = "Paulina cómo estás? Por favor me puedes transferir 350.000 a FFMM? muchas gracias!!";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // Original type: 6 (Consulta sobre Productos y Servicios / otros productos y servicio)
            sample.Contents = "Jasna, buenas tardes.  Agradeceré su apoyo para indicar como desbloquear tarjeta de coordenadas desde el extranjero. No la pude utilizar porque intenté tres veces y aunque estoy seguro que estaba ingresando las coordenadas correctas, se bloqueo por intento.  Si quiere contactarse conmigo, tiene que ser por whatsapp al número +52 5548845595  Muchas gracias.";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} ===============");

            // Original type: 7 Reclamo (problema con un producto) / Cuenta Corriente y Servicios
            sample.Contents = "Tengo firmado un documento para que no se me descuente de manera automática desde mi cuenta corriente para cubrir mi linea de sobregiro. Eso lo manejo yo, su banco siempre incurre en el mismo error. Cuántas veces hay que recordarlo??? Necesito se solucione y que de claro este tema a la brevedad. Gracias";
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

        public static void PredictComment()
        {
            // <SnippetLoadModel>
            ITransformer loadedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
            // </SnippetLoadModel>

            // <SnippetAddTestIssue> 
            // Correct Type: 5
            BankCustomerMessage sample = new BankCustomerMessage() { Contents = "Tengo firmado un documento para que no se me descuente de manera automática desde mi cuenta corriente para cubrir mi linea de sobregiro. Eso lo manejo yo, su banco" };
            // </SnippetAddTestIssue> 

            //Predict label for single hard-coded issue
            // <SnippetCreatePredictionEngine>
            _predEngine = _mlContext.Model.CreatePredictionEngine<BankCustomerMessage, BankMessageTypePrediction>(loadedModel);
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

    /// <summary>
    /// BankMessageSubtypeModelBuilder
    /// Subtype prediction
    /// </summary>
    public class BankMessageSubtypeModelBuilder
    {
        public static string _basePath = @"C:\Users\rodri\Documents\GitHub\RDemos\DemoModelBuilder\DemoModelBuilder";
        const string dataFolder = "data";

        private static string _sampleDataPath => Path.Combine(_basePath, dataFolder, "SampleBankComments.tsv");
        private static string _modelPath => Path.Combine(_basePath, dataFolder, "BankCommentSubtypeModel.zip");

        private static MLContext _mlContext;
        private static PredictionEngine<BankCustomerMessage, BankMessageTypePrediction> _predEngine;
        private static ITransformer _trainedModel;
        static IDataView _samplesDataView;

        public BankMessageSubtypeModelBuilder()
        { }

        public void Build()
        {
            _mlContext = new MLContext(seed: 0);

            _samplesDataView = _mlContext.Data.LoadFromTextFile<BankCustomerMessage>(_sampleDataPath, hasHeader: true);

            DataOperationsCatalog.TrainTestData dataSplit = _mlContext.Data.TrainTestSplit(_samplesDataView, testFraction: 0.2);
            IDataView _trainingDataView = dataSplit.TrainSet;
            IDataView _testDataView = dataSplit.TestSet;

            var pipeline = ProcessData();

            var trainingPipeline = BuildAndTrainModel(_trainingDataView, pipeline);

            Evaluate(_trainingDataView.Schema, _testDataView);

            PredictComment();
        }
        public static IEstimator<ITransformer> ProcessData()
        {
            Console.WriteLine($"=============== Processing Data ===============");
            // STEP 2: Common data process configuration with pipeline data transformations
            // <SnippetMapValueToKey>
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "SubTypeCode", outputColumnName: "Label")
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
            _predEngine = _mlContext.Model.CreatePredictionEngine<BankCustomerMessage, BankMessageTypePrediction>(_trainedModel);
            // </SnippetCreatePredictionEngine1>

            // Original subtype: 24 (Solicitar transferencia (mismo banco/ otros bancos)
            BankCustomerMessage sample = new BankCustomerMessage();
            sample.Contents = "Necesito transferir Urgente me encuentro fuera de Chile y debo transferir dinero para el Conservador de Bienes Raices por un valor de $11.400 Debo enviar un correo";
            var prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} (24) ===============");

            // Original type: 14 (Solicitar Tarjeta Débito adicional / para tercera persona)
            sample.Contents = "Hola Grace. Necesito solicitar tarjetas adicionales de mi redcompra. Como lo hago?";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} (14) ===============");

            // Original type: 30 (Solicitar información general cuenta corriente)
            sample.Contents = "Hola Lydia estoy con deuda en línea de crédito y tarjeta de crédito. Que tiene más intereses. Cuál deuda es mejor saldar antes.   Saludos muchas gracias ";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} (30) ===============");

            // Original type: 7 (Solicitar cartola de movimientos de Cuenta corriente)
            sample.Contents = "Hola Gabriela,   a qué corresponde el cargo del 12 de septiembre a nombre de   P.A.C. ZENIT SEGUROS GRALES SA-FRAUDE por $ 4.016 ?  Tengo entendido que no he tomado un ";
            prediction = _predEngine.Predict(sample);
            Console.WriteLine($"Prediction just-trained-model - {sample.Contents}\nResult: {prediction.TypeCode} (7) ===============");

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

        public static void PredictComment()
        {
            // <SnippetLoadModel>
            ITransformer loadedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
            // </SnippetLoadModel>

            // <SnippetAddTestIssue> 
            // Correct Type: 5
            BankCustomerMessage sample = new BankCustomerMessage() { Contents = "Felipe.    Buenas tardes. Te comento que revisando mi estado de cuenta me encuentro que nuevamente se cargo el valor duplicado correspondiente a un seguro por " };
            // </SnippetAddTestIssue> 

            //Predict label for single hard-coded issue
            // <SnippetCreatePredictionEngine>
            _predEngine = _mlContext.Model.CreatePredictionEngine<BankCustomerMessage, BankMessageTypePrediction>(loadedModel);
            // </SnippetCreatePredictionEngine>

            // <SnippetPredictIssue>
            var prediction = _predEngine.Predict(sample);
            // </SnippetPredictIssue>

            // <SnippetDisplayResults>
            Console.WriteLine($"=============== Single Prediction - Result: {prediction.TypeCode} (7) ===============");
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
